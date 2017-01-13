using System;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentRevisions
{
    internal class RBTreeRevisions<TKey, TValue> : Revisions<TValue>
        where TKey : IComparable<TKey>, IComparable, IEquatable<TKey>
    {
        private RBTreeNode<TKey, TValue> root;

        public RBTreeRevisions()
        {
            root = new RBTreeNode<TKey, TValue>(Thread.CurrentThread.ManagedThreadId);
        }

        public void Add(TKey key, TValue value)
        {
            var ver = (RBTreeNode<TKey, TValue>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            ver.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            var ver = (RBTreeNode<TKey, TValue>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            var ver = (RBTreeNode<TKey, TValue>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.ContainsKey(key);
        }

        public TValue this[TKey key]
        {
            get
            {
                var ver = (RBTreeNode<TKey, TValue>)GetVersion(Thread.CurrentThread.ManagedThreadId);
                return ver[key];
            }
            set
            {
                var ver = (RBTreeNode<TKey, TValue>)GetVersion(Thread.CurrentThread.ManagedThreadId);
                ver[key] = value;
            }
        }

        public int Count
        {
            get
            {
                var ver = (RBTreeNode<TKey, TValue>)GetVersion(Thread.CurrentThread.ManagedThreadId);
                return ver.Count;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var ver = (RBTreeNode<TKey, TValue>)GetVersion(Thread.CurrentThread.ManagedThreadId);
                return ver.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var ver = (RBTreeNode<TKey, TValue>)GetVersion(Thread.CurrentThread.ManagedThreadId);
                return ver.Values;
            }
        }

        protected override RevisionNode GetVersion(int id)
        {
            return GetVersion(id, root);
        }

        protected override RevisionNode CreateNode(int id, NodeType type, RevisionNode parent)
        {
            return new RBTreeNode<TKey, TValue>(id, type, (RBTreeNode<TKey, TValue>)parent);
        }

        protected override void ApplyOperations(RevisionNode mergedVer, System.Collections.Generic.Stack<Operation> mergeOps, Func<TValue, TValue, TValue> mergeValueRule)
        {
            var ver = (RBTreeNode<TKey, TValue>)mergedVer;

            while (mergeOps.Count > 0)
            {
                var op = mergeOps.Pop();
                switch (op.Type)
                {
                    case OperationType.Remove:
                        ver.Remove((TKey)op.Value);
                        break;

                    case OperationType.Edit:
                        var updated = (Tuple<TKey, TValue, TValue>)op.Value;
                        if (!ver.ContainsKey(updated.Item1))
                            ver.Add(updated.Item1, updated.Item3);
                        else
                            ver[updated.Item1] = mergeValueRule(ver[updated.Item1], updated.Item3);
                        break;
                }
            }
        }
    }
}
