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
    class HexStringTests : DataModelCollector
    {
        [Test]
        public void Test1()
        {
            // standard test

            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                "<Charon>" +
                "   <DataModel name=\"TheDataModel\">" +
                "       <Block name=\"TheBlock\">" +
                "           <Transformer class=\"HexString\"/>" +
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
            byte[] precalcResult = new byte[] { 0x34, 0x38, 0x20, 0x36, 0x35, 0x20, 0x36, 0x63, 0x20, 0x36, 0x63, 0x20, 0x36, 0x66 };
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(precalcResult, values[0].Value);
        }
    }
}

// end