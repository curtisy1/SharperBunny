using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SharperBunny.Utils {
    public class AsyncManualResetEvent {
        private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool> ();

        public Task WaitAsync () {
            return _tcs.Task;
        }

        public void Set () {
            _tcs.TrySetResult (result: true);
        }

        public void Reset () {
            while (true) {
                var tcs = _tcs;
                if (!tcs.Task.IsCompleted ||
                    Interlocked.CompareExchange (ref _tcs, new TaskCompletionSource<bool> (), tcs) == tcs) {
                    return;
                }
            }
        }
    }
}