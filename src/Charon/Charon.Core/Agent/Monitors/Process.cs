
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
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Charon.Core.Dom;

using NLog;

using System.Runtime.InteropServices;

namespace Charon.Core.Agent.Monitors
{
	/// <summary>
	/// Start a process
	/// </summary>
	[Monitor("Process", true)]
	[Monitor("process.Process")]
	[Parameter("Executable", typeof(string), "Executable to launch")]
	[Parameter("Arguments", typeof(string), "Optional command line arguments", "")]
	[Parameter("RestartOnEachTest", typeof(bool), "Restart process for each interation", "false")]
	[Parameter("FaultOnEarlyExit", typeof(bool), "Trigger fault if process exists", "true")]
	[Parameter("NoCpuKill", typeof(bool), "Disable process killing when CPU usage nears zero", "false")]
	[Parameter("StartOnCall", typeof(string), "Start command on state model call", "")]
	[Parameter("WaitForExitOnCall", typeof(string), "Wait for process to exit on state model call and fault if timeout is reached", "")]
	[Parameter("WaitForExitTimeout", typeof(int), "Wait for exit timeout value in milliseconds (-1 is infinite)", "10000")]
	public class Process : Monitor
	{
		/*
			引入charonControl库，进行共享内存相关操作
		*/
		[DllImport(@"charonControl", EntryPoint="newPath")]
        public static unsafe extern int newPath();

        [DllImport(@"charonControl", EntryPoint="clear_trace_bits")]
        public static unsafe extern void clear_trace_bits();  
  
		[DllImport(@"charonControl", EntryPoint="count_branch")]
        public static unsafe extern int count_branch();   

		[DllImport(@"charonControl", EntryPoint="hash_after_classify")]
        public static unsafe extern int hash_after_classify(); 

		[DllImport(@"charonControl", EntryPoint="termination_detection_init")]
        public static unsafe extern void termination_detection_init(); 

		[DllImport(@"charonControl", EntryPoint="termination_detection")]
        public static unsafe extern int termination_detection();
		[DllImport(@"charonControl", EntryPoint="init")]   
		public static unsafe extern int init();

		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		System.Diagnostics.Process _process = null;
		Fault _fault = null;
		bool _messageExit = false;
		uint iterationCount;
		bool isReproduction;

		static int pid = 0;
		static string asan_log_path;
		public string Executable { get; private set; }
		public string Arguments { get; private set; }
		public bool RestartOnEachTest { get; private set; }
		public bool FaultOnEarlyExit { get; private set; }
		public bool NoCpuKill { get; private set; }
		public string StartOnCall { get; private set; }
		public string WaitForExitOnCall { get; private set; }
		public int WaitForExitTimeout { get; private set; }

		public Process(IAgent agent, string name, Dictionary<string, Variant> args)
			: base(agent, name, args)
		{
			ParameterParser.Parse(this, args);
		}

		void _Start()
		{
			// init();
			// clear_trace_bits();
			System.Environment.SetEnvironmentVariable("ASAN_OPTIONS", "abort_on_error=1:detect_leaks=0:symbolize=1:allocator_may_return_null=1:" + "log_path=" + Charon.Core.Runtime.SHARE.pathAsanReport);
			System.Environment.SetEnvironmentVariable("MSAN_OPTIONS", "exit_code=86:msan_track_origins=0:symbolize=1:abort_on_error=1:allocator_may_return_null=1");
			if (_process == null || _process.HasExited)
			{
				if (_process != null)
					_process.Close();

				_process = new System.Diagnostics.Process();
				_process.StartInfo.FileName = Executable;
				_process.StartInfo.UseShellExecute = false;

				if (!string.IsNullOrEmpty(Arguments))
					_process.StartInfo.Arguments = Arguments;

				logger.Debug("_Start(): Starting process");

				try
				{
					_process.Start();
				}
				catch (Exception ex)
				{
					_process = null;
					throw new CharonException("Could not start process '" + Executable + "'.  " + ex.Message + ".", ex);
				}
				pid = _process.Id;
			}
			else
			{
				logger.Debug("_Start(): Process already running, ignore");
			}
			asan_log_path = Charon.Core.Runtime.SHARE.pathAsanReport + "." + pid.ToString();
			if(File.Exists(asan_log_path))
			{
				Console.WriteLine("Deleting the Former Asan Report:" + asan_log_path);
				System.IO.File.Delete(asan_log_path);
			}

			// //等到共享内存不变化
			// Thread.Sleep(1000);
			// //判断待测程序是否执行完
			// Console.WriteLine("Checking whether the program has started ......");
			// // int cur_cksum = hash_after_classify();
			// // int last_cksum = cur_cksum + 1;
			// int cnt = 0;

			// // _Stop();

			// // Thread.Sleep(1000);

			// // termination_detection_init();
			// // while(termination_detection() != 0)


			// // newPath();
			// // count_branch();
			
			// while(true)
			// {
			// 	int zeroTimes=0;
			// 	while(zeroTimes<3){
			// 		Thread.Sleep(100);
			// 		int hnb = newPath();
			// 		int branch = count_branch();
			// 		// int branch = 0;
			// 		// last_cksum = cur_cksum;
			// 		// cur_cksum = hash_after_classify();
			// 		cnt++;
			// 		Console.WriteLine("Checking iteration {0} ..{2}.. {1} ", cnt, branch, hnb);
			// 		if(hnb==0){
			// 			zeroTimes++;
			// 		}
					
			// 	}
			// 	clear_trace_bits();
			// 	Thread.Sleep(2000);
				
			// }

			// Console.WriteLine("Program has started after {0} times of check......", cnt + 1);
			// clear_trace_bits();

		}

		void _Stop()
		{
			logger.Debug("_Stop()");

			for (int i = 0; i < 100 && _IsRunning(); i++)
			{
				logger.Debug("_Stop(): Killing process");
				try
				{
					_process.Kill();
					_process.WaitForExit();
					_process.Close();
					_process = null;
				}
				catch (Exception ex)
				{
					logger.Error("_Stop(): {0}", ex.Message);
				}
			}

			if (_process != null)
			{
				logger.Debug("_Stop(): Closing process handle");
				_process.Close();
				_process = null;
			}
			else
			{
				logger.Debug("_Stop(): _process == null, done!");
			}
		}

		void _WaitForExit(bool useCpuKill)
		{
			if (!_IsRunning())
				return;

			if (useCpuKill && !NoCpuKill)
			{
				const int pollInterval = 200;
				ulong lastTime = 0;
				int i = 0;

				try
				{
					for (i = 0; i < WaitForExitTimeout; i += pollInterval)
					{
						var pi = ProcessInfo.Instance.Snapshot(_process);

						logger.Trace("CpuKill: OldTicks={0} NewTicks={1}", lastTime, pi.TotalProcessorTicks);

						if (i != 0 && lastTime == pi.TotalProcessorTicks)
						{
							logger.Debug("Cpu is idle, stopping process.");
							break;
						}

						lastTime = pi.TotalProcessorTicks;
						Thread.Sleep(pollInterval);
					}

					if (i >= WaitForExitTimeout)
						logger.Debug("Timed out waiting for cpu idle, stopping process.");
				}
				catch (Exception ex)
				{
					logger.Debug("Error querying cpu time: {0}", ex.Message);
				}

				_Stop();
			}
			else
			{
				logger.Debug("WaitForExit({0})", WaitForExitTimeout == -1 ? "INFINITE" : WaitForExitTimeout.ToString());

				if (!_process.WaitForExit(WaitForExitTimeout))
				{
					if (!useCpuKill)
					{
						logger.Debug("FAULT, WaitForExit ran out of time!");
						_fault = MakeFault("ProcessFailedToExit", "Process did not exit in " + WaitForExitTimeout + "ms");
						this.Agent.QueryMonitors("CanaKitRelay_Reset");
					}
				}
			}
		}

		bool _IsRunning()
		{
			return _process != null && !_process.HasExited;
		}

		Fault MakeFault(string folder, string reason)
		{
			return new Fault()
			{
				type = FaultType.Fault,
				detectionSource = "ProcessMonitor",
				title = reason,
				description = "{0}: {1} {2}".Fmt(reason, Executable, Arguments),
				folderName = folder,
			};
		}

		public override void IterationStarting(uint iterationCount, bool isReproduction)
		{
			this.iterationCount = iterationCount;
			this.isReproduction = isReproduction;

			_fault = null;
			_messageExit = false;

			// if (!_messageExit && !RestartOnEachTest && FaultOnEarlyExit && !_IsRunning())
			// {
			// 	_fault = MakeFault("ProcessExitedEarly", "Process exited early");
			// 	_Stop();
			// }

			if (RestartOnEachTest)
				_Stop();

						if (StartOnCall == null && RestartOnEachTest)
				_Start();
		}

		public override bool DetectedFault()
		{
			return _fault != null;
		}

		public override Fault GetMonitorData()
		{
			return _fault;
		}

		public override bool MustStop()
		{
			return false;
		}

		public override void StopMonitor()
		{
			_Stop();
		}

		public override void SessionStarting()
		{
			if (StartOnCall == null && !RestartOnEachTest)
				_Start();
		}

		public override void SessionFinished()
		{
			_Stop();
		}

		public override bool IterationFinished()
		{
			Console.WriteLine("charon: iterationFinished status: " + iterationCount + isReproduction +  "_messageExit: " + _messageExit + 
			" FaultOnEarlyExit:" + FaultOnEarlyExit + " _IsRunning:" + _IsRunning());

			
			if (!_messageExit && FaultOnEarlyExit && !_IsRunning())
			{
				logger.Info("DetectedFault");
				if(File.Exists(asan_log_path))
				{
					_fault = MakeFault("AsanCrash", "Detect Asan Crash");
					Console.WriteLine("Fetching Asan Report from " + asan_log_path);
					byte[] bytes = File.ReadAllBytes(asan_log_path);
					_fault.collectedData["Asan_Report.txt"] = bytes;
					if(Charon.Core.Runtime.SHARE.pathAsanReport.Equals(@"/tmp/CharonAsanReport"))
					{
						System.IO.File.Delete(asan_log_path);
					}
				}
				else
				{
					_fault = MakeFault("ProcessExitedEarly", "Process exited early");
				}

				_Stop();
								_Start();
			}
			else  if (StartOnCall != null)
			{
				_WaitForExit(true);
				_Stop();
			}
			else if (RestartOnEachTest)
			{
				_Stop();
			}

			if(_fault ==  null && File.Exists(asan_log_path))	// Asan Crash Detected
			{
				_fault = MakeFault("AsanCrash", "Detect Asan Crash");
				logger.Info("DetectedFault");
				Console.WriteLine("Fetching Asan Report from " + asan_log_path);
				byte[] bytes = File.ReadAllBytes(asan_log_path);
				_fault.collectedData["Asan_Report.txt"] = bytes;
				if(Charon.Core.Runtime.SHARE.pathAsanReport.Equals(@"/tmp/CharonAsanReport"))
				{
					System.IO.File.Delete(asan_log_path);
				}
				
				_Stop();
				//如果崩溃后 重新start
				_Start();
			}

			return true;
		}

		public override Variant Message(string name, Variant data)
		{
			logger.Debug("Message(" + name + ", " + (string)data + ")");

			if (name == "Action.Call" && ((string)data) == StartOnCall)
			{
				_Stop();
				_Start();
			}
			else if (name == "Action.Call" && ((string)data) == WaitForExitOnCall)
			{
				_messageExit = true; 
				_WaitForExit(false);
				_Stop();
			}
			else
			{
				logger.Debug("Unknown msg: " + name + " data: " + (string)data);
			}

			return null;
		}
	}
}

// end
