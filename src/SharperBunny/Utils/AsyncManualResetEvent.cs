namespace SharperBunny.Utils {
  using System.Threading;
  using System.Threading.Tasks;

  public class AsyncManualResetEvent {
    private volatile TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

    public Task WaitAsync() {
      return this._tcs.Task;
    }

    public void Set() {
      this._tcs.TrySetResult(true);
    }

    public void Reset() {
      while (true) {
        var tcs = this._tcs;
        if (!tcs.Task.IsCompleted ||
            Interlocked.CompareExchange(ref this._tcs, new TaskCompletionSource<bool>(), tcs) == tcs) {
          return;
        }
      }
    }
  }
}