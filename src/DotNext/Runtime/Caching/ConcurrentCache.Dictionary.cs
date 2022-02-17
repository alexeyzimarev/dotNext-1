using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DotNext.Runtime.Caching;

public partial class ConcurrentCache<TKey, TValue>
{
    [DebuggerDisplay($"Key = {{{nameof(Key)}}} Value = {{{nameof(Value)}}}")]
    private abstract class KeyValuePair
    {
        internal readonly int KeyHashCode;
        internal readonly TKey Key;
        internal volatile KeyValuePair? Next;
        internal (KeyValuePair? Previous, KeyValuePair? Next) Links;

        private protected KeyValuePair(TKey key, int hashCode, int buffersCount)
        {
            Key = key;
            KeyHashCode = hashCode;
        }

        internal abstract TValue Value { get; set; }

        internal void Clear()
        {
            Links = default;
            Next = null;
        }
    }

    private sealed class KeyValuePairAtomicAccess : KeyValuePair
    {
        internal KeyValuePairAtomicAccess(TKey key, TValue value, int hashCode, int buffersCount)
            : base(key, hashCode, buffersCount)
            => Value = value;

        internal override TValue Value { get; set; }
    }

    // non-atomic access utilizes copy-on-write semantics
    private sealed class KeyValuePairNonAtomicAccess : KeyValuePair
    {
        private object value;

        internal KeyValuePairNonAtomicAccess(TKey key, TValue value, int hashCode, int buffersCount)
            : base(key, hashCode, buffersCount)
            => this.value = value!;

        internal override TValue Value
        {
            get => (TValue)value;
            set => this.value = value!;
        }
    }

    private readonly KeyValuePair?[] buckets;
    private readonly object[] locks;
    private readonly IEqualityComparer<TKey>? keyComparer;
    private volatile int count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref KeyValuePair? GetBucket(int hashCode)
    {
        var index = (uint)hashCode % (uint)buckets.Length;

        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(buckets), index);
    }

    private ref KeyValuePair? GetBucket(int hashCode, out object bucketLock)
    {
        var index = (uint)hashCode % (uint)buckets.Length;

        bucketLock = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(locks), index);
        return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(buckets), index);
    }

    private void Remove(KeyValuePair expected)
    {
        ref var bucket = ref GetBucket(expected.KeyHashCode, out var bucketLock);

        lock (bucketLock)
        {
            for (KeyValuePair? actual = Volatile.Read(ref bucket), previous = null; actual is not null; previous = actual, actual = actual.Next)
            {
                if (ReferenceEquals(expected, actual))
                {
                    if (previous is null)
                        Volatile.Write(ref bucket, actual.Next);
                    else
                        previous.Next = actual.Next;

                    OnRemoved();
                    break;
                }
            }
        }
    }

    private void OnAdded() => Interlocked.Increment(ref count);

    private void OnRemoved() => Interlocked.Decrement(ref count);

    private bool TryAdd(TKey key, TValue value, bool updateIfExists, out TValue? previous)
    {
        var keyComparer = this.keyComparer;
        var hashCode = keyComparer?.GetHashCode(key) ?? key.GetHashCode();
        ref var bucket = ref GetBucket(hashCode, out var bucketLock);
        bool result;
        CommandType command;
        KeyValuePair pair;

        lock (bucketLock)
        {
            if (keyComparer is null)
            {
                for (KeyValuePair? current = Volatile.Read(ref bucket); current is not null; current = current.Next)
                {
                    if (hashCode == current.KeyHashCode && (typeof(TKey).IsValueType ? EqualityComparer<TKey>.Default.Equals(key, current.Key) : current.Key.Equals(key)))
                    {
                        previous = current.Value;
                        result = false;
                        if (updateIfExists)
                        {
                            current.Value = value;
                            command = CommandType.Read;
                            pair = current;
                            goto enqueue_and_exit;
                        }

                        goto exit;
                    }
                }
            }
            else
            {
                for (KeyValuePair? current = Volatile.Read(ref bucket); current is not null; current = current.Next)
                {
                    if (hashCode == current.KeyHashCode && keyComparer.Equals(key, current.Key))
                    {
                        previous = current.Value;
                        result = false;
                        if (updateIfExists)
                        {
                            current.Value = value;
                            command = CommandType.Read;
                            pair = current;
                            goto enqueue_and_exit;
                        }

                        goto exit;
                    }
                }
            }

            previous = default;
            pair = IsValueWriteAtomic
                ? new KeyValuePairAtomicAccess(key, value, hashCode, concurrencyLevel)
                : new KeyValuePairNonAtomicAccess(key, value, hashCode, concurrencyLevel);
            pair.Next = bucket;
            Volatile.Write(ref bucket, pair);
            command = CommandType.Add;
            result = true;
            OnAdded();
        }

    enqueue_and_exit:
        EnqueueAndDrain(command, pair);

    exit:
        return result;
    }

    /// <summary>
    /// Gets or sets cache entry.
    /// </summary>
    /// <param name="key">The key of the cache entry.</param>
    /// <returns>The cache entry.</returns>
    /// <exception cref="KeyNotFoundException">The cache entry with <paramref name="key"/> doesn't exist.</exception>
    public TValue this[TKey key]
    {
        get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
        set => TryAdd(key, value, true, out _);
    }

    /// <summary>
    /// Adds a new cache entry if the cache is not full.
    /// </summary>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="value">The cache entry.</param>
    /// <returns><see langword="true"/> if the entry is added successfully; otherwise, <see langword="false"/>.</returns>
    public bool TryAdd(TKey key, TValue value) => TryAdd(key, value, false, out _);

    /// <summary>
    /// Adds or modifies the cache entry as an atomic operation.
    /// </summary>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="value">The cache entry.</param>
    /// <param name="added">
    /// <see langword="true"/> if a new entry is added;
    /// <see langword="false"/> if the existing entry is modified.
    /// </param>
    /// <returns>
    /// <paramref name="value"/> if <paramref name="added"/> is <see langword="true"/>;
    /// or the value before modification.
    /// </returns>
    public TValue AddOrUpdate(TKey key, TValue value, out bool added)
    {
        if (added = TryAdd(key, value, true, out var result))
            result = value;

        return result!;
    }

    /// <summary>
    /// Gets or adds the cache entry as an atomic operation.
    /// </summary>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="value">The cache entry.</param>
    /// <param name="added">
    /// <see langword="true"/> if a new entry is added;
    /// <see langword="false"/> if the entry is already exist.
    /// </param>
    /// <returns>
    /// <paramref name="value"/> if <paramref name="added"/> is <see langword="true"/>;
    /// or existing value.
    /// </returns>
    public TValue GetOrAdd(TKey key, TValue value, out bool added)
    {
        TValue? result;
        if (TryGetValue(key, out result))
        {
            added = false;
        }
        else if (added = TryAdd(key, value, false, out result))
        {
            result = value;
        }

        return result!;
    }

    /// <summary>
    /// Attempts to get existing cache entry.
    /// </summary>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="value">The cache entry, if successful.</param>
    /// <returns><see langword="true"/> if the cache entry exists; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var keyComparer = this.keyComparer;
        int hashCode;
        KeyValuePair? pair;

        if (keyComparer is null)
        {
            hashCode = key.GetHashCode();
            for (pair = Volatile.Read(ref GetBucket(hashCode)); pair is not null; pair = pair.Next)
            {
                if (hashCode == pair.KeyHashCode && (typeof(TKey).IsValueType ? EqualityComparer<TKey>.Default.Equals(key, pair.Key) : pair.Key.Equals(key)))
                {
                    EnqueueAndDrain(CommandType.Read, pair);
                    value = pair.Value;
                    return true;
                }
            }
        }
        else
        {
            hashCode = keyComparer.GetHashCode(key);
            for (pair = Volatile.Read(ref GetBucket(hashCode)); pair is not null; pair = pair.Next)
            {
                if (hashCode == pair.KeyHashCode && keyComparer.Equals(key, pair.Key))
                {
                    EnqueueAndDrain(CommandType.Read, pair);
                    value = pair.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Attempts to remove the cache entry.
    /// </summary>
    /// <remarks>
    /// This method will not raise <see cref="OnEviction"/> event for the removed entry.
    /// </remarks>
    /// <param name="key">The key of the cache entry.</param>
    /// <param name="value">The cache entry, if successful.</param>
    /// <returns><see langword="true"/> if the cache entry removed successfully; otherwise, <see langword="false"/>.</returns>
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        var keyComparer = this.keyComparer;
        var hashCode = keyComparer?.GetHashCode(key) ?? key.GetHashCode();
        ref var bucket = ref GetBucket(hashCode, out var bucketLock);
        bool result;
        KeyValuePair pair;

        lock (bucketLock)
        {
            if (keyComparer is null)
            {
                for (KeyValuePair? current = Volatile.Read(ref bucket), previous = null; current is not null; previous = current, current = current.Next)
                {
                    if (hashCode == current.KeyHashCode && (typeof(TKey).IsValueType ? EqualityComparer<TKey>.Default.Equals(key, current.Key) : current.Key.Equals(key)))
                    {
                        result = true;
                        pair = current;
                        if (previous is null)
                            Volatile.Write(ref bucket, current.Next);
                        else
                            previous.Next = current.Next;

                        OnRemoved();
                        value = current.Value;
                        goto enqueue_and_exit;
                    }
                }
            }
            else
            {
                for (KeyValuePair? current = Volatile.Read(ref bucket), previous = null; current is not null; previous = current, current = current.Next)
                {
                    if (hashCode == current.KeyHashCode && keyComparer.Equals(key, current.Key))
                    {
                        result = true;
                        pair = current;
                        if (previous is null)
                            Volatile.Write(ref bucket, current.Next);
                        else
                            previous.Next = current.Next;

                        OnRemoved();
                        value = current.Value;
                        goto enqueue_and_exit;
                    }
                }
            }

            value = default;
            result = false;
            goto exit;
        }

    enqueue_and_exit:
        EnqueueAndDrain(CommandType.Remove, pair);

    exit:
        return result;
    }

    private int AcquireAllLocks()
    {
        int i;
        for (i = 0; i < locks.Length; i++)
            Monitor.Enter(locks[i]);

        return i;
    }

    private void ReleaseLocks(int acquiredLocks)
    {
        for (var i = 0; i < acquiredLocks; i++)
            Monitor.Exit(locks[i]);
    }

    private void RemoveAllKeys()
    {
        foreach (ref var root in buckets.AsSpan())
        {
            for (var current = Volatile.Read(ref root); current is not null; current = current.Next)
                current.Clear();

            Volatile.Write(ref root, null);
        }

        count = 0;
    }
}