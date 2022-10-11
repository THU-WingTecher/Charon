﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CharonValidator
{

#if MONO
	static class Extensions
	{
		public static void BeginInit(this SplitContainer cont)
		{
		}

		public static void EndInit(this SplitContainer cont)
		{
		}
	}
#endif

	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
	}
}
