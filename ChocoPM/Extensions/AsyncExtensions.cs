using System;
using System.Threading;

namespace ChocoPM.Extensions
{
    public static class AsyncExtensions
    {
        public static void DelayedExecution(this Action action, TimeSpan delay)
        {
            Timer timer = null;
            var context = SynchronizationContext.Current;

            timer = new Timer((c) =>
            {
                timer.Dispose();
                context.Post(spc => action(), null);
            }, null, delay, TimeSpan.FromMilliseconds(-1));
        }
    }
}
