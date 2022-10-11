﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Charon.Core;
using Charon.Core.Dom;
using Charon.Core.Analyzers;

namespace Charon.Core.Test.Transformers.Encode
{
    [TestFixture]
    class Base64DecodeTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test (internal encode)

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Charon>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Block name=\"TheBlock\">" +
                "           <Transformer class=\"Base64Decode\"/>" +
                "           <Blob name=\"Data\" value=\"12345678\"/>" +
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
            // -- this is the pre-calculated result from Charon2.3 on the blob: "12345678"
            byte[] precalcResult = new byte[] { 0xD7, 0x6D, 0xF8, 0xE7, 0xAE, 0xFC };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcResult, values[0].Value);
        }
    }
}

// end
