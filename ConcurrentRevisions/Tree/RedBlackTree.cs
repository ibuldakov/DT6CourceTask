using System;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentRevisions
{
    public class RedBlackTree<TKey, TValue> : VersionedCollection<TValue>
        where TKey : IComparable<TKey>, IComparable, IEquatable<TKey>
    {
        public RedBlackTree()
        {
            revisions = new RBTreeRevisions<TKey, TValue>();
        }

        public override int Count
        {
            get
            {
                return revisions.Count;
            }
        }

        public ICollection<TKey> Keys
        {
            get { return revisions.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return revisions.Values; }
        }

        public void Add(TKey key, TValue value)
        {
            revisions.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return revisions.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            return revisions.ContainsKey(key);
        }

        public TValue this[TKey key]
        {
            get { return revisions[key]; }
            set { revisions[key] = value; }
        }

        internal override void Fork(int id)
        {
            revisions.Fork(id);
        }

        public override bool CanJoin(int id)
        {
            return revisions.CanJoin(Thread.CurrentThread.ManagedThreadId, id);
        }

        public override void Join(int id, Func<System.Collections.Generic.Stack<Operation>, System.Collections.Generic.Stack<Operation>, System.Collections.Generic.Stack<Operation>> mergeRule, Func<TValue, TValue, TValue> mergeValueRule = null)
        {
            revisions.Join(Thread.CurrentThread.ManagedThreadId, id, mergeRule, mergeValueRule);
        }

        private RBTreeRevisions<TKey, TValue> revisions;
    }
}
