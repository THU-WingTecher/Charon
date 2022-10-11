﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Charon.Core;
using Charon.Core.Dom;
using Charon.Core.Analyzers;
using Charon.Core.Cracker;
using Charon.Core.IO;

namespace Charon.Core.Test.Transformers.Encode
{
	[TestFixture]
	class HexTests : DataModelCollector
	{
		byte[] precalcResult = new byte[] { 0x34, 0x38, 0x36, 0x35, 0x36, 0x63, 0x36, 0x63, 0x36, 0x66 };

		[Test]
		public void Test1()
		{
			// standard test

			string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
				"<Charon>" +
				"   <DataModel name=\"TheDataModel\">" +
				"       <Block name=\"TheBlock\">" +
				"           <Transformer class=\"Hex\"/>" +
				"           <Blob name=\"Data\" value=\"Hello\"/>" +
				"       </Block>" +
				"   </DataModel>" +

				"   <StateModel name=\"TheState\" initialState=\"Initial\">" +
				"       <State name=\"Initial\">" +
				"           <Action type=\"output\">" +
				"               <DataModel ref=\"TheDataModel\"/>" +
				"           </Action>" +
				"       </State>" +
				"   </StateModel>" +

				"   <Test name=\"Default\">" +
				"       <StateModel ref=\"TheState\"/>" +
				"       <Publisher class=\"Null\"/>" +
				"   </Test>" +
				"</Charon>";

			PitParser parser = new PitParser();

			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			// verify values
			// -- this is the pre-calculated result from Charon2.3 on the blob: "Hello"
			Assert.AreEqual(1, values.Count);
			Assert.AreEqual(precalcResult, values[0].Value);
		}

		[Test]
		public void CrackTest()
		{
			string xml = @"
<Charon>
	<DataModel name='DM'>
		<String/>
		<Transformer class='Hex'/>
	</DataModel>
</Charon>
";

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			BitStream data = new BitStream();
			data.LittleEndian();
			data.WriteBytes(precalcResult);
			data.SeekBits(0, SeekOrigin.Begin);

			DataCracker cracker = new DataCracker();
			cracker.CrackData(dom.dataModels[0], data);

			Assert.AreEqual("Hello", (string)dom.dataModels[0][0].DefaultValue);
		}
	}
}

// end
