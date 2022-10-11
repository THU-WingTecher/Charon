using System;
using System.Collections;
using System.Collections.Generic;
using Charon.Core;
using Charon.Core.Agent.Monitors;
using NUnit.Framework;
using System.Threading;

namespace Charon.Core.Test.Agent.Monitors
{
	[TestFixture]
	public class CrashReporterTest
	{
		[Test]
		public void NoProcessNoFault()
		{
			// ProcessName argument not provided to the monitor
			// When no crashing program is run, the monitor should not detect a fault

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			string Charon = "";
			string process = null;
			bool shouldFault = false;

			RunProcess(Charon, process, shouldFault, args);
		}

		[Test]
		public void NoProcessFault()
		{
			// ProcessName argument not provided to the monitor
			// When crashing program is run, the monitor should detect a fault

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			string Charon = "qwertyuiopasdfghjklzxcvbnm";
			string process = "CrashingProgram";
			bool shouldFault = true;

			Fault fault = RunProcess(Charon, process, shouldFault, args);

			Assert.NotNull(fault);
			Assert.Greater(fault.collectedData.Count, 0);
			foreach (var item in fault.collectedData)
			{
				Assert.NotNull(item.Key);
				Assert.Greater(item.Value.Length, 0);
			}
		}

		[Test]
		public void ProcessFault()
		{
			// Correct ProcessName argument is provided to the monitor
			// When crashing program is run, the monitor should detect a fault

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["ProcessName"] = new Variant("CrashingProgram");
			string Charon = "qwertyuiopasdfghjklzxcvbnm";
			string process = "CrashingProgram";
			bool shouldFault = true;

			Fault fault = RunProcess(Charon, process, shouldFault, args);

			Assert.NotNull(fault);
			Assert.Greater(fault.collectedData.Count, 0);
			foreach (var item in fault.collectedData)
			{
				Assert.NotNull(item.Key);
				Assert.Greater(item.Value.Length, 0);
			}
		}

		[Test]
		public void WrongProcessFault()
		{
			// Incorrect ProcessName argument is provided to the monitor
			// When crashing program is run, the monitor should not detect a fault

			Dictionary<string, Variant> args = new Dictionary<string, Variant>();
			args["ProcessName"] = new Variant("WrongCrashingProgram");
			string Charon = "qwertyuiopasdfghjklzxcvbnm";
			string process = "CrashingProgram";
			bool shouldFault = false;

			RunProcess(Charon, process, shouldFault, args);
		}

		private static Fault RunProcess(string Charon, string process, bool shouldFault, Dictionary<string, Variant> args)
		{
			CrashReporter reporter = new CrashReporter(null, "name", args);
			reporter.SessionStarting();
			reporter.IterationStarting(0, false);
			if (process != null)
			{
				using (System.Diagnostics.Process p = new System.Diagnostics.Process())
				{
					p.StartInfo = new System.Diagnostics.ProcessStartInfo();
					p.StartInfo.EnvironmentVariables["Charon"] = Charon;
					p.StartInfo.UseShellExecute = false;
					p.StartInfo.FileName = process;
					p.Start();
				}
			}
			Thread.Sleep(2000);
			reporter.IterationFinished();
			Assert.AreEqual(shouldFault, reporter.DetectedFault());
			Fault fault = reporter.GetMonitorData();
			reporter.StopMonitor();
			return fault;
		}
	}
}

