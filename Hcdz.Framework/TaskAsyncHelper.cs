 
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Hcdz.ModulePcie
{
    internal static class TaskAsyncHelper
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This is a shared file")]
        public static Task Delay(TimeSpan timeOut)
        {
#if NETFX_CORE || PORTABLE
            return Task.Delay(timeOut);
#else
            var tcs = new TaskCompletionSource<object>();

            var timer = new Timer(tcs.SetResult,
            null,
            timeOut,
            TimeSpan.FromMilliseconds(-1));

            return tcs.Task.ContinueWith(_ =>
            {
                timer.Dispose();
            },
            TaskContinuationOptions.ExecuteSynchronously);
#endif
        }
    }
}