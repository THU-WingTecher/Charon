namespace Charon.Core.Debuggers.DebugEngine.Tlb
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    public struct _DEBUG_LAST_EVENT_INFO_EXCEPTION
    {
        public _EXCEPTION_RECORD64 ExceptionRecord;
        public uint FirstChance;
    }
}

