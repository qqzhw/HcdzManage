using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using Jungo.wdapi_dotnet; 
using BYTE = System.Byte;
using WORD = System.UInt16;
using DWORD = System.UInt32;
using UINT32 = System.UInt32;
using UINT64 = System.UInt64;

namespace Hcdz.PcieLib
{
    public class Log
    {
        public delegate void TRACE_LOG(string str);
        public delegate void ERR_LOG(string str);

        public static TRACE_LOG dTraceLog;
        public static ERR_LOG dErrLog;

        public Log(TRACE_LOG funcTrace, ERR_LOG funcErr)
        {
            dTraceLog = funcTrace;
            dErrLog = funcErr;
        }

        public static void TraceLog(string str)
        {
            if (dTraceLog == null)
                return;

            dTraceLog(str);
        }

        public static void ErrLog(string str)
        {
            if (dErrLog == null)
                return;

            dErrLog(str);
        }
    }
}



