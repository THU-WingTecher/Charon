using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using NLog;
using Charon.Core;
using System.Runtime.CompilerServices;

namespace Charon.Core
{
	public abstract class SingleInstance : PlatformFactory<SingleInstance>, IDisposable
	{
		public abstract void Dispose();
		public abstract bool TryLock();
		public abstract void Lock();
	}
}
