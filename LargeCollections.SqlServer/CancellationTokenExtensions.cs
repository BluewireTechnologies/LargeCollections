using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LargeCollections.SqlServer
{
    internal static class CancellationTokenExtensions
    {
        public static CancellationTokenAwaiter GetAwaiter(this CancellationToken cancellationToken)
        {
            return new CancellationTokenAwaiter(cancellationToken);
        }

        public readonly struct CancellationTokenAwaiter : INotifyCompletion
        {
            private readonly CancellationToken cancellationToken;

            public CancellationTokenAwaiter(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public bool IsCompleted => cancellationToken.IsCancellationRequested;

            public void OnCompleted(Action continuation) => cancellationToken.Register(continuation);

            public void GetResult() => cancellationToken.WaitHandle.WaitOne();
        }
    }
}
