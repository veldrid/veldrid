using System;
using System.Collections.Generic;

namespace Veldrid.Vk
{
    internal class VkRefCountManager
    {
        private uint _lastId;
        private Stack<uint> _idPool;
        private SparseDictionary<int, Action> _refCounts;

        public VkRefCountManager()
        {
            _idPool = new Stack<uint>();
            _refCounts = new SparseDictionary<int, Action>(1024);
        }

        public uint Register(Action disposeAction)
        {
            SparseDictionary<int, Action> refCounts = _refCounts;
            lock (refCounts)
            {
                uint id;
                if (_idPool.Count > 0)
                {
                    id = _idPool.Pop();
                }
                else
                {
                    id = _lastId++;
                }

                refCounts.Add(id);
                refCounts.Values1[id] = 1;
                refCounts.Values2[id] = disposeAction;
                return id;
            }
        }

        public void Increment(uint refCountId)
        {
            SparseDictionary<int, Action> refCounts = _refCounts;
            lock (refCounts)
            {
                ref int count = ref refCounts.Values1[refCountId];
                count++;
#if VALIDATE_USAGE
                if (count == 0)
                {
                    ThrowHelper_ObjectDisposed();
                }
#endif
            }
        }

        public void Increment<TEnumerator>(TEnumerator refCountIds)
            where TEnumerator : IEnumerator<uint>
        {
            SparseDictionary<int, Action> refCounts = _refCounts;
            lock (refCounts)
            {
                int[] counts = refCounts.Values1;

                while (refCountIds.MoveNext())
                {
                    ref int count = ref counts[refCountIds.Current];
                    count++;
#if VALIDATE_USAGE
                    if (count == 0)
                    {
                        ThrowHelper_ObjectDisposed();
                    }
#endif
                }
            }
        }

        public void Decrement(uint refCountId)
        {
            SparseDictionary<int, Action> refCounts = _refCounts;
            Stack<uint> idPool = _idPool;
            lock (refCounts)
            {
                ref int count = ref refCounts.Values1[refCountId];
                count--;
                if (count == 0)
                {
                    refCounts.Values2[refCountId].Invoke();
                    refCounts.Remove(refCountId);
                    idPool.Push(refCountId);
                }
            }
        }

        public void Decrement<TEnumerator>(TEnumerator refCountIds)
            where TEnumerator : IEnumerator<uint>
        {
            SparseDictionary<int, Action> refCounts = _refCounts;
            Stack<uint> idPool = _idPool;
            lock (refCounts)
            {
                int[] counts = refCounts.Values1;
                Action[] actions = refCounts.Values2;

                while (refCountIds.MoveNext())
                {
                    uint refCountId = refCountIds.Current;
                    ref int count = ref counts[refCountId];
                    count--;
                    if (count == 0)
                    {
                        actions[refCountId].Invoke();
                        refCounts.Remove(refCountId);
                        idPool.Push(refCountId);
                    }
                }
            }
        }

        private static void ThrowHelper_ObjectDisposed()
        {
            throw new VeldridException("An attempt was made to reference a disposed resource.");
        }
    }
}
