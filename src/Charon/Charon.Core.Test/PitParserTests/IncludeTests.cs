using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
//using NUnit.Framework.Constraints;
using Charon.Core;
using Charon.Core.Dom;
using Charon.Core.Analyzers;

namespace Charon.Core.Test.PitParserTests
{
	[TestFixture]
	class IncludeTests
	{
		[Test]
		public void Test1()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Charon>
	<DataModel name=""HelloWorldTemplate"">
		<String name=""str"" value=""Hello World!""/>
		<String>
			<Relation type=""size"" of=""HelloWorldTemplate""/>
		</String>
	</DataModel>
</Charon>
";

			string template = @"
<Charon>
	<Include ns=""example"" src=""{0}"" />

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel name=""foo"" ref=""example:HelloWorldTemplate"" />
			</Action>
		</State>
	</StateModel>
	
	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Charon>";
			
			string remote = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote, output);

			using (TextWriter writer = File.CreateText(remote))
			{
				writer.Write(inc1);
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string result = File.ReadAllText(output);

			Assert.AreEqual("Hello World!13", result);
		}

		[Test]
		public void Test2()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Charon>

	<DataModel name=""BaseModel"">
		<String name=""str"" value=""Hello World!""/>
	</DataModel>

	<DataModel name=""HelloWorldTemplate"" ref=""BaseModel"">
	</DataModel>
</Charon>
";

			string template = @"
<Charon>
	<Include ns=""example"" src=""file:{0}"" />

	<DataModel name=""DM"">
		<Block ref=""example:HelloWorldTemplate""/>
	</DataModel>

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""DM"" />
			</Action>
		</State>
	</StateModel>
	
	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Charon>";

			string remote = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote, output);

			using (TextWriter writer = File.CreateText(remote))
			{
				writer.Write(inc1);
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string result = File.ReadAllText(output);

			Assert.AreEqual("Hello World!", result);
		}

		[Test]
		public void Test3()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Charon>
	<DataModel name=""BaseModel"">
		<String name=""str"" value=""Hello World!""/>
	</DataModel>

	<DataModel name=""DerivedModel"">
		<Block ref=""BaseModel"" />
	</DataModel>
</Charon>
";

			string inc2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Charon>
	<Include ns=""abc"" src=""file:{0}"" />

	<DataModel name=""BaseModel2"" ref=""abc:DerivedModel""/>

</Charon>
";

			string template = @"
<Charon>
	<Include ns=""example"" src=""file:{0}"" />

	<DataModel name=""DM"">
		<Block ref=""example:BaseModel2""/>
	</DataModel>

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""DM"" />
			</Action>
		</State>
	</StateModel>
	
	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Charon>";

			string remote1 = Path.GetTempFileName();
			string remote2 = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote2, output);

			using (TextWriter writer = File.CreateText(remote1))
			{
				writer.Write(inc1);
			}

			using (TextWriter writer = File.CreateText(remote2))
			{
				writer.Write(string.Format(inc2, remote1));
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			string result = File.ReadAllText(output);

			Assert.AreEqual("Hello World!", result);
		}

		[Test]
		public void Test4()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Charon>
	<DataModel name=""HelloWorldTemplate"">
		<Number name=""Size"" size=""8"">
			<Relation type=""size"" of=""HelloWorldTemplate""/>
		</Number>
		<String name=""str"" value=""four""/>
	</DataModel>
</Charon>
";

			string template = @"
<Charon>
	<Include ns=""example"" src=""{0}"" />

	<DataModel name='Foo'>
		<String name='slurp' value='slurping' />
	</DataModel>

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""example:HelloWorldTemplate"" />
			</Action>
		</State>
	</StateModel>
	
	<StateModel name=""StateOverride"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""example:HelloWorldTemplate"" />
				<Data>
					<Field name=""str"" value=""hello""/>
				</Data>
			</Action>
		</State>
	</StateModel>

	<StateModel name=""StateSlurp"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""slurp"" valueXpath=""//slurp"" setXpath=""//str"">
				<DataModel ref=""Foo"" />
			</Action>

			<Action type=""output"">
				<DataModel ref=""example:HelloWorldTemplate"" />
			</Action>
		</State>
	</StateModel>

	<Test name=""Slurp"">
		<StateModel ref=""StateSlurp"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>

	<Test name=""Override"">
		<StateModel ref=""StateOverride"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>

	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""File"">
			<Param name=""FileName"" value=""{1}""/>
		</Publisher>
	</Test>
	
</Charon>";

			string remote = Path.GetTempFileName();
			string output = Path.GetTempFileName();

			string xml = string.Format(template, remote, output);

			using (TextWriter writer = File.CreateText(remote))
			{
				writer.Write(inc1);
			}

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, dom.tests[2], config);

			byte[] result = File.ReadAllBytes(output);

			Assert.AreEqual(Encoding.ASCII.GetBytes("\x0005four"), result);

			dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			e = new Engine(null);
			e.startFuzzing(dom, dom.tests[1], config);

			result = File.ReadAllBytes(output);

			Assert.AreEqual(Encoding.ASCII.GetBytes("\x0006hello"), result);

			dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));
			e = new Engine(null);
			e.startFuzzing(dom, dom.tests[0], config);

			result = File.ReadAllBytes(output);

			Assert.AreEqual(Encoding.ASCII.GetBytes("\x0009slurping"), result);
		}

		[Test]
		public void Test5()
		{
			string inc1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Charon>
	<DataModel name=""Model1"">
		<String name=""str1"" value=""foo""/>
	</DataModel>

	<DataModel name=""Model2"">
		<String name=""str2"" value=""bar""/>
	</DataModel>
</Charon>
";

			string template = @"
<Charon>
	<Include ns=""example"" src=""{0}"" />

	<StateModel name=""State"" initialState=""Initial"">
		<State name=""Initial"">
			<Action type=""output"">
				<DataModel ref=""example:Model1"" />
			</Action>

			<Action type=""slurp"" valueXpath=""//example:Model1/str1"" setXpath=""//example:Model2//str2"" />

			<Action type=""output"">
				<DataModel ref=""example:Model2"" />
			</Action>
		</State>
	</StateModel>

	<Test name=""Default"">
		<StateModel ref=""State"" />
		<Publisher class=""Null""/>
	</Test>
</Charon>";

			string remote = Path.GetTempFileName();
			string xml = string.Format(template, remote);
			File.WriteAllText(remote, inc1);

			PitParser parser = new PitParser();
			Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

			RunConfiguration config = new RunConfiguration();
			config.singleIteration = true;

			Engine e = new Engine(null);
			e.startFuzzing(dom, config);

			var act = dom.tests[0].stateModel.states["Initial"].actions[2];
			Assert.AreEqual("foo", (string)act.dataModel[0].DefaultValue);
		}
	}
}

