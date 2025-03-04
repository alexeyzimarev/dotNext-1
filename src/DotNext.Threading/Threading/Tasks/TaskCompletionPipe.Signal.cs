using System.Diagnostics;

namespace DotNext.Threading.Tasks;

using Pooling;

public partial class TaskCompletionPipe<T>
{
    private sealed class Signal : LinkedValueTaskCompletionSource<bool>, IPooledManualResetCompletionSource<Action<Signal>>
    {
        private Action<Signal>? completionCallback;

        protected override void AfterConsumed() => completionCallback?.Invoke(this);

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal bool NeedsRemoval => CompletionData is null;

        ref Action<Signal>? IPooledManualResetCompletionSource<Action<Signal>>.OnConsumed => ref completionCallback;
    }

    private ValueTaskPool<bool, Signal, Action<Signal>> pool;
    private LinkedValueTaskCompletionSource<bool>? first, last;

    private void RemoveNode(LinkedValueTaskCompletionSource<bool> signal)
    {
        Debug.Assert(Monitor.IsEntered(this));

        if (ReferenceEquals(signal, first))
            first = signal.Next;

        if (ReferenceEquals(signal, last))
            last = signal.Previous;

        signal.Detach();
    }

    private void Notify()
    {
        Debug.Assert(Monitor.IsEntered(this));

        for (LinkedValueTaskCompletionSource<bool>? current = first, next; current is not null; current = next)
        {
            next = current.Next;

            RemoveNode(current);
            if (current.TrySetResult(Sentinel.Instance, value: true))
                break;
        }
    }

    private void DrainWaitQueue()
    {
        Debug.Assert(Monitor.IsEntered(this));

        for (LinkedValueTaskCompletionSource<bool>? current = first, next; current is not null; current = next)
        {
            next = current.CleanupAndGotoNext();
            current?.TrySetResult(value: false);
        }

        first = last = null;
    }

    private LinkedValueTaskCompletionSource<bool> EnqueueNode()
    {
        Debug.Assert(Monitor.IsEntered(this));

        LinkedValueTaskCompletionSource<bool> result = pool.Get();

        if (last is null)
        {
            first = last = result;
        }
        else
        {
            last.Append(result);
            last = result;
        }

        return result;
    }
}