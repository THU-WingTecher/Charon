
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Xml;

using Charon.Core.Dom;
using Charon.Core;
using Charon.Core.Agent;
using Charon.Core.Analyzers;

using SharpPcap;
using NLog;
using NLog.Targets;
using NLog.Config;
using System.Threading;

using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Charon.Core.Runtime
{
	/// <summary>
	/// Command line interface for Charon 3.  Mostly backwards compatable with
	/// Charon 2.3.
	/// </summary>


	
	//SHARE class, store what we want to mutate, and the legal values of func. field.
	public class SHARE{
		public static bool ifuse = false;
		public static string pathSSrc = @"/tmp/charonBranch";
		public static string pathSrc = @"/tmp/charonPath";
		public static string pathWather = @"/tmp/charonWather";
		public static string pathAsanReport = @"/tmp/";		// Directory to save ASAN report
		public static Queue<DataModel> dataModelsToMutate = new Queue<DataModel>();
		public static Queue<DataModel> valuableDataModels = new Queue<DataModel>();
		public static int queueLengthBeforeIteration = 0;

		public static int seed_pool_to_use_cnt = 0; 	// in this iteration, number of seeds to use in seed pool

		//当前action是不是有new path
		public static bool has_new_path = false;

		//在has_new_path的情况下进一步细化是不是new branch
		public static bool has_new_path_branch = false;

		public static bool has_new_path_iteration = false;  // true: if has new path in this iteration(including sub_iteration)

		public static int cur_path = 0;

		public static DateTime last_path_time;
		
		public static int average_path_time = 0 ;

		public static int use_time_limit = 10;

		public static int seed_pool_to_use_cnt_limit = 3;

		//是否使用概率学
		public static bool usep = false;

		public static bool if_replace_just_now = false;

		public static bool if_in = false;

		public static uint CurIteration = 0;

		public static uint CurSubIteration = 0;

		//复现采用的bin文件
		public static string repro = null;

		public static Queue<int> seedPoolIndexQueue = new Queue<int>();
		public static int seedPoolIndex = 0;

		public static Queue<int> seedPoolIndexQueueCopy = new Queue<int>(); //用于Crash时保存的SeedIndexQueue快照

		public static bool if_CharonStarRepo = false;

		public static int CharonStarRepoStartIteration;







		//DAC 2021 DynCharon

		//当前iteration变异了哪些字段
		public static List<Tuple<string,string>> mutation_element = new List<Tuple<string, string>>();
		
		// Element 和 触发新code的对应关系表
		public static Dictionary<Tuple<string,string>,double[]> element_interesting = new Dictionary<Tuple<string,string>, double[]>();

		public static Dictionary<Tuple<string,string>,int> element_mutation_weight = new Dictionary<Tuple<string, string>, int>();

		//全局Relation Table
		public static Dictionary<Tuple<string,string>,Dictionary<Tuple<string,string>,int>> relation_table = new Dictionary<Tuple<string, string>, Dictionary<Tuple<string, string>, int>>();

		//临时标志，标志现在previous_datamodel起作用
		public static DataModel previous_datamodel = null;

		//临时标志，标志是否应该进行relation_learning
		public static bool should_relation_learn = false;

		public static Int64 time_iteration = 0;

		public static bool if_use_dyn = false;

		public static bool if_use_relation_learning = false;

		public static bool if_ban_guided = false;



		//Charon重构
		//目前执行了多少action
		public static int totalAction = 0;
		//目前有多少action执行了inState策略
		public static int totalInStateAction = 0;
		//目前有多少action执行了CrossState策略
		public static int totalCrossStateAction = 0;
		//有多少Action触发了新branch/path
		public static int totalInterestingAction = 0;
		//有多少instate策略的action触发了新branch/path
		public static int totalInStateInterestingAction = 0;
		//当前action有没有执行in state策略
		public static bool ifInStateStrategyWorks=false;
		//当前action有没有执行cross state策略
		public static bool ifCrossStateStrategyWorks=false;
		//当前action有没有尝试执行cross state策略
		public static bool ifCrossStateStrategyTryToWork=false;
		//当前action中上一轮Cross state策略是否在起作用
		public static bool ifCrossStateStrategyServes=false;
		//有多少crossState策略的action触发了新branch/path
		public static int totalCrossStateInterestingAction = 0;

		//程序开始时间
		public static DateTime programStartTime = DateTime.UtcNow;

 


		public static int saveNewSeedToFile(DataModel dataModel,Charon.Core.Loggers.FileLogger logger){
			
			//保存新的Seed(valuableDataModel)进入文件系统
			Charon.Core.Loggers.FileLogger fileLogger = logger; 
			string seedPoolPath = fileLogger.OurPath + "/seedpool";
			Console.WriteLine("charon:seedPoolPath:{0}",seedPoolPath);
			if (!Directory.Exists(seedPoolPath))
				Directory.CreateDirectory(seedPoolPath);
			string seedFilePath = seedPoolPath + "/" + Charon.Core.Runtime.SHARE.seedPoolIndex + ".bin";
			FileStream fs = new FileStream (seedFilePath, FileMode.Create);
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (fs, dataModel);
			fs.Close ();

			return 0;
		}

		public static int saveSeedQueueIndexToFile(string path){
			
			string filepath = path + "/seedPoolIndex.bin";
			FileStream fs = new FileStream (filepath, FileMode.Create);
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (fs, seedPoolIndexQueueCopy);
			fs.Close ();

			return 0;			

		}

		public static int readSeedPoolFromFile(string filepath,string indexpath){
			//从文件系统中读出SeedPool
			//首先读出index
			string indexFilePath = indexpath + "/seedPoolIndex.bin";
			FileStream fs = new FileStream (indexFilePath, FileMode.Open);
			BinaryFormatter bf = new BinaryFormatter ();
			seedPoolIndexQueue = bf.Deserialize (fs) as Queue<int>;
			fs.Close ();

			seedPoolIndexQueueCopy = new Queue<int>(seedPoolIndexQueue);

			valuableDataModels = new Queue<DataModel>();

			while(seedPoolIndexQueueCopy.Count!=0){
				int thisIndex = seedPoolIndexQueueCopy.Peek();
				seedPoolIndexQueueCopy.Dequeue();

				string thisSeedPath = filepath + "/" + thisIndex.ToString() + ".bin";
				FileStream newfs = new FileStream (thisSeedPath, FileMode.Open);
				BinaryFormatter newbf = new BinaryFormatter ();
				DataModel dataModel = newbf.Deserialize(newfs) as DataModel;
				newfs.Close();
				valuableDataModels.Enqueue(dataModel);
			}

			seedPoolIndexQueueCopy = new Queue<int>(seedPoolIndexQueue);

			return 0;
		}

	}


	public class Program
	{
		// PUT THIS INTO YOUR PROGRAM
		////public static int Run(string[] args)
		////{
		////    Charon.Core.AssertWriter.Register();

		////    return new Program(args).exitCode;
		////}

		public static ConsoleColor DefaultForground = Console.ForegroundColor;

		public Dictionary<string, string> DefinedValues = new Dictionary<string,string>();
		public Charon.Core.Dom.Dom dom;

		public int exitCode = 1;

		/// <summary>
		/// Copyright message
		/// </summary>
		public virtual string Copyright
		{
			get { return ""; }
		}

		/// <summary>
		/// Product name
		/// </summary>
		public virtual string ProductName
		{
			get { return "Charon (based onCharon v3.0)"; }
		}

		/// <summary>
		/// Error on 64 vs 32bit missmatch? Override to change.
		/// </summary>
		public virtual bool ErrorOnArchitecture { get { return true; } }

				[DllImport(@"charonControl", EntryPoint="charon_read")]
        public static unsafe extern int charon_read(char[] path, ref int iteration);
		[DllImport(@"charonControl", EntryPoint="init")]
		public static unsafe extern int init();

		public Program(string[] args)
		{
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_DomainUnload);
			Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
			RunConfiguration config = new RunConfiguration();
			config.debug = false;

			try
			{
				string analyzer = null;
				bool test = false;
				string agent = null;
				var definedValues = new List<string>();
				bool parseOnly = false;

				var color = Console.ForegroundColor;
				Console.Write("\n");
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.Write("[[ ");
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine(ProductName);
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.Write("[[ ");
				Console.ForegroundColor = ConsoleColor.DarkCyan;
				Console.WriteLine(Copyright);
				Console.WriteLine();
				Console.ForegroundColor = color;

				if (args.Length == 0)
					Syntax();

				var p = new OptionSet()
				{
					{ "h|?|help", v => Syntax() },
					{ "analyzer=", v => analyzer = v },
					{ "debug", v => config.debug = true },
					{ "1", v => config.singleIteration = true},
					{ "range=", v => ParseRange(config, v)},
					{ "t|test", v => test = true},
					{ "c|count", v => config.countOnly = true},
					{ "skipto=", v => config.skipToIteration = Convert.ToUInt32(v)},
					{ "seed=", v => config.randomSeed = Convert.ToUInt32(v)},
					{ "p|parallel=", v => ParseParallel(config, v)},
					{ "a|agent=", v => agent = v},
					{ "D|define=", v => AddNewDefine(v) },
					{ "definedvalues=", v => definedValues.Add(v) },
					{ "parseonly", v => parseOnly = true },
					{ "bob", var => bob() },
					{ "charlie", var => Charlie() },
					{ "showdevices", var => ShowDevices() },
					{ "showenv", var => ShowEnvironment() },
					{ "pro", v => SHARE.ifuse = true },
					{ "pathp=", v => SHARE.pathSrc = v },
					{ "salva=", v => SHARE.seed_pool_to_use_cnt_limit = Convert.ToInt32(v) },
					{ "pathb=", v => SHARE.pathSSrc = v },
					{ "usep" , v => SHARE.usep = true},
					{ "repro=", v => SHARE.repro = v},
					{ "asanLog=", v => SHARE.pathAsanReport = v},
					{ "usedyn" , v => SHARE.if_use_dyn = true},
					{ "uselearning" , v => SHARE.if_use_relation_learning = true},
					{ "banguided" , v => SHARE.if_ban_guided = true},
				};

				List<string> extra = p.Parse(args);

				if(!SHARE.pathAsanReport.Equals("stderr"))	
				{	// The special value for 'asanLog', output the ASAN report to stderr (Note: The ASAN crash can not be captured if output to stderr)
					if(!Directory.Exists(SHARE.pathAsanReport))
					{
						Console.WriteLine("Error, unable to open the asanLog directory \'" + SHARE.pathAsanReport + "\'.");	
						return;
					}
					if(!SHARE.pathAsanReport.EndsWith("/"))
					{
						SHARE.pathAsanReport = SHARE.pathAsanReport + "/charonAsanReport";
					}
					else
					{
						SHARE.pathAsanReport = SHARE.pathAsanReport + "charonAsanReport";
					}
				}


				int suffix=0;
								while(System.IO.File.Exists(SHARE.pathSrc) || System.IO.File.Exists(SHARE.pathSSrc)){
					SHARE.pathSrc = SHARE.pathSrc + "_dup_" + suffix.ToString();
					SHARE.pathSSrc = SHARE.pathSSrc + "_dup_" + suffix.ToString();
					suffix++;
					// System.IO.File.Delete(SHARE.pathSrc);
					// Console.WriteLine("exist");
				}
				Console.WriteLine("charon:  "+SHARE.pathSrc);
				Console.WriteLine("charon:  "+SHARE.pathSSrc);	
				Console.WriteLine("ifuse:  "+SHARE.ifuse);	
				// System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(SHARE.pathSrc);
				// di.Create();
				// if(System.IO.File.Exists(SHARE.pathSSrc)){
				// 	System.IO.File.Delete(SHARE.pathSSrc);
				// 	// Console.WriteLine("exist");
				// }

				if(System.IO.File.Exists(SHARE.pathWather)){
					System.IO.File.Delete(SHARE.pathWather);
				}

				if(init() == 0)
				{
					Console.WriteLine("Error, unable to locate the shared memory. Please set env \'SHM_ENV_VAR\'.");	
					return;
				}
								if(SHARE.repro != null ){
					
					// if(SHARE.ifuse == false){
					// 						// 	Console.WriteLine("charon:use repo but not use pro!");
					// 	return;
					// }
					SHARE.if_CharonStarRepo = true;
					

					if(config.range == false){
												Console.WriteLine("charon:use repo but not use range!");
						return;
					}
					
					Console.WriteLine("charon:charon_read start");
					unsafe{
						int read_iteration=0;
						if(0 != charon_read((SHARE.repro + "/repo.bin").ToCharArray(), ref read_iteration)){
							Console.WriteLine("charon: charon_read run error!");
							return;
						}
						SHARE.CharonStarRepoStartIteration = read_iteration;
												if(config.rangeStart > (uint)read_iteration){
							config.rangeStart = (uint)read_iteration;
						}
						Console.WriteLine("charon:charon_read {0} {1} {2}",read_iteration,config.rangeStart,config.rangeStop );
					}
					Console.WriteLine("charon:charon_read end");

					SHARE.readSeedPoolFromFile(SHARE.repro + "/../../../seedpool/",SHARE.repro);
				}
				

				if (extra.Count == 0 && agent == null && analyzer == null)
					Syntax();

				Platform.LoadAssembly();

				AddNewDefine("Charon.Cwd=" + Environment.CurrentDirectory);

				foreach (var definedValuesFile in definedValues)
				{
					var defs = PitParser.parseDefines(definedValuesFile);

					foreach (var kv in defs)
					{
						// Allow command line to override values in XML file.
						if (!DefinedValues.ContainsKey(kv.Key))
							DefinedValues.Add(kv.Key, kv.Value);
					}
				}

				// Enable debugging if asked for
				if (config.debug)
				{
					var nconfig = new LoggingConfiguration();
					var consoleTarget = new ColoredConsoleTarget();
					nconfig.AddTarget("console", consoleTarget);
					consoleTarget.Layout = "${logger} ${message}";

					var rule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
					nconfig.LoggingRules.Add(rule);

					LogManager.Configuration = nconfig;
				}

				if (agent != null)
				{
					var agentType = ClassLoader.FindTypeByAttribute<AgentServerAttribute>((x, y) => y.name == agent);
					if (agentType == null)
					{
						Console.WriteLine("Error, unable to locate agent server for protocol '" + agent + "'.\n");
						return;
					}

					var agentServer = Activator.CreateInstance(agentType) as IAgentServer;

					ConsoleWatcher.WriteInfoMark();
					Console.WriteLine("Starting agent server");

					agentServer.Run(new Dictionary<string, string>());
					return;
				}

				if (analyzer != null)
				{
					var analyzerType = ClassLoader.FindTypeByAttribute<AnalyzerAttribute>((x, y) => y.Name == analyzer);
					if (analyzerType == null)
					{
						Console.WriteLine("Error, unable to locate analyzer called '" + analyzer + "'.\n");
						return;
					}

					var field = analyzerType.GetField("supportCommandLine",
						BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
					if ((bool)field.GetValue(null) == false)
					{
						Console.WriteLine("Error, analyzer not configured to run from command line.");
						return;
					}

					var analyzerInstance = Activator.CreateInstance(analyzerType) as Analyzer;

					ConsoleWatcher.WriteInfoMark();
					Console.WriteLine("Starting Analyzer");

					analyzerInstance.asCommandLine(new Dictionary<string, string>());
					return;
				}

				Dictionary<string, object> parserArgs = new Dictionary<string, object>();
				parserArgs[PitParser.DEFINED_VALUES] = this.DefinedValues;

				if (test)
				{
					ConsoleWatcher.WriteInfoMark();
					Console.Write("Validating file [" + extra[0] + "]... ");
					Analyzer.defaultParser.asParserValidation(parserArgs, extra[0]);

					if (Type.GetType("Mono.Runtime") != null)
						Console.WriteLine("File parsed successfully, but XSD validation is not supported on the Mono runtime.");
					else
						Console.WriteLine("No Errors Found.");

					return;
				}

				Engine e = new Engine(GetUIWatcher());
				dom = GetParser(e).asParser(parserArgs, extra[0]);
				config.pitFile = extra[0];

				// Used for unittests
				if (parseOnly)
					return;

				foreach (string arg in args)
					config.commandLine += arg + " ";

				if (extra.Count > 1)
				{
					if (!dom.tests.ContainsKey(extra[1]))
						throw new CharonException("Error, unable to locate test named \"" + extra[1] + "\".");

					e.startFuzzing(dom, dom.tests[extra[1]], config);
				}
				else
					e.startFuzzing(dom, config);

				exitCode = 0;
			}
			catch (SyntaxException)
			{
				// Ignore, thrown by syntax()
			}
			catch (OptionException oe)
			{
					Console.WriteLine(oe.Message +"\n"); 
			}
			catch (CharonException ee)
			{
				if (config.debug)
					Console.WriteLine(ee);
				else
					Console.WriteLine(ee.Message + "\n");
			}
			finally
			{
				// HACK - Required on Mono with NLog 2.0
				LogManager.Configuration = null;

				// Reset console colors
				Console.ForegroundColor = DefaultForground;
			}
		}

		protected static void CurrentDomain_DomainUnload(object sender, EventArgs e)
		{
			Console.ForegroundColor = DefaultForground;
		}

		/// <summary>
		/// Override to add custom options
		/// </summary>
		/// <param name="options"></param>
		protected virtual void AddCustomOptions(OptionSet options)
		{
		}

		/// <summary>
		/// Override to change syntax message.
		/// </summary>
		protected virtual void Syntax()
		{
			string syntax = @"This is the Charon Runtime (based on Charon v3.0).  The Runtime is one of the many ways
to use XML Model files.  Currently this runtime is still in development
but already exposes several abilities to the end-user such as performing
simple fuzzer runs and performing parsing tests of Charon XML files.


Syntax:

  charon -a channel
  charon -c xml_file [test_name]
  charon [--skipto #] xml_flie [test_name]
  charon -p 10,2 [--skipto #] xml_file [test_name]
  charon --range 100,200 xml_file [test_name]
  charon -t xml_file

  -1                         Perform a single iteration
  -a,--agent                 Launch Agent
  -c,--count                 Count test cases
  -t,--test xml_file         Validate a XML file (in Charon Mode)
  -p,--parallel M,N          Parallel fuzzing.  Total of M machines, this
                             is machine N.
  --debug                    Enable debug messages. Usefull when debugging
                             your XML file.  Warning: Messages are very
                             cryptic sometimes.
  --seed N                   Sets the seed used by the random number generator
  --parseonly                Test parse a XML file
  --showenv                  Print a list of all DataElements, Fixups, Monitors
                             Publishers and their associated parameters.
  --showdevices              Display the list of PCAP devices
  --skipto N                 Skip to a specific test #.  This replaced -r
                             for restarting a Charon run.
  --range N,M                Provide a range of test #'s to be run.
  -D/define=KEY=VALUE        Define a substitution value.  In your PIT you can
                             ##KEY## and it will be replaced for VALUE.
  --definedvalues=FILENAME   XML file containing defined values


Charon Agent

  Syntax: charon -a channel
  
  Starts up a Charon Agent instance on this current machine.  User must provide
  a channel/protocol name (e.g. tcp).

  Note: Local agents are started automatically.

Performing Fuzzing Run

  Syntax: charon xml_flie [test_name]
  Syntax: charon --skipto 1234 xml_flie [test_name]
  Syntax: charon --range 100,200 xml_flie [test_name]
  
  A fuzzing run is started by by specifying the XML file and the
  name of a test to perform.
  
  If a run is interupted for some reason it can be restarted using the
  --skipto parameter and providing the test # to start at.
  
  Additionally a range of test cases can be specified using --range.

Debug XML File

  Syntax: charon -1 --debug xml_file

  This will perform a single iteration (-1) of your pit file while displaying
  alot of debugging information (--debug).  The debugging information was
  origionally intended just for the developers, but can be usefull in pit
  debugging as well.
";
			Console.WriteLine(syntax);
			throw new SyntaxException();
		}

		protected void bob()
		{
			string bob = @"
";
			Console.WriteLine(bob);
			throw new SyntaxException();
		}


		protected void Charlie()
		{
			Console.WriteLine(@"
");
			throw new SyntaxException();
		}

		protected void ParseRange(RunConfiguration config, string v)
		{
			string[] parts = v.Split(',');
			if (parts.Length != 2)
				throw new CharonException("Invalid range: " + v);

			try
			{
				config.rangeStart = Convert.ToUInt32(parts[0]);
			}
			catch (Exception ex)
			{
				throw new CharonException("Invalid range start iteration: " + parts[0], ex);
			}

			try
			{
				config.rangeStop = Convert.ToUInt32(parts[1]);
			}
			catch (Exception ex)
			{
				throw new CharonException("Invalid range stop iteration: " + parts[1], ex);
			}

			if (config.parallel)
				throw new CharonException("--range is not supported when --parallel is specified");

			config.range = true;
		}

		protected void ParseParallel(RunConfiguration config, string v)
		{
			string[] parts = v.Split(',');
			if (parts.Length != 2)
				throw new CharonException("Invalid parallel value: " + v);

			try
			{
				config.parallelTotal = Convert.ToUInt32(parts[0]);

				if (config.parallelTotal == 0)
					throw new ArgumentOutOfRangeException();
			}
			catch (Exception ex)
			{
				throw new CharonException("Invalid parallel machine total: " + parts[0], ex);
			}

			try
			{
				config.parallelNum = Convert.ToUInt32(parts[1]);
				if (config.parallelNum == 0 || config.parallelNum > config.parallelTotal)
					throw new ArgumentOutOfRangeException();
			}
			catch (Exception ex)
			{
				throw new CharonException("Invalid parallel machine number: " + parts[1], ex);
			}

			if (config.range)
				throw new CharonException("--parallel is not supported when --range is specified");

			config.parallel = true;
		}

		protected static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine();
			Console.WriteLine(" --- Ctrl+C Detected --- ");

			e.Cancel = true;

			// Need to call Environment.Exit from outside this event handler
			// to ensure the finalizers get called...
			// http://www.codeproject.com/Articles/16164/Managed-Application-Shutdown
			new Thread(delegate()
			{
				Environment.Exit(0);
			}).Start();
		}

		public void AddNewDefine(string value)
		{
			if(value.IndexOf("=") < 0)
				throw new CharonException("Error, defined values supplied via -D/--define must have an equals sign providing a key-pair set.");

			var kv = value.Split('=');
			DefinedValues[kv[0]] = kv[1];
		}

		public void ShowDevices()
		{
			Console.WriteLine();
			Console.WriteLine("The following devices are available on this machine:");
			Console.WriteLine("----------------------------------------------------");
			Console.WriteLine();

			int i = 0;

			var devices = CaptureDeviceList.Instance;

			// Print out all available devices
			foreach (ICaptureDevice dev in devices)
			{
				Console.WriteLine("Name: {0}\nDescription: {1}\n\n", dev.Name, dev.Description);
				i++;
			}

			throw new SyntaxException();
		}

		public void ShowEnvironment()
		{
			Charon.Core.Usage.Print();
			throw new SyntaxException();
		}

		protected virtual Watcher GetUIWatcher()
		{
			return new ConsoleWatcher();
		}

		protected virtual Analyzer GetParser(Engine engine)
		{
			return Analyzer.defaultParser;
		}
	}


	public class SyntaxException : Exception
	{
	}
}

// end
