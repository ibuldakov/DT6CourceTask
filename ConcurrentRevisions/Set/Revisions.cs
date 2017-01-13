using System;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentRevisions
{
    internal class SetRevisions<T> : Revisions<T>
    {
        private SetNode<T> root;

        public SetRevisions()
        {
            root = new SetNode<T>(Thread.CurrentThread.ManagedThreadId);
        }

        public SetRevisions(IEqualityComparer<T> comparer)
        {
            root = new SetNode<T>(Thread.CurrentThread.ManagedThreadId, comparer);
            Comparer = comparer;
        }

        public bool Add(T item)
        {
            var ver = (SetNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.Add(item);
        }

        public bool Contains(T item)
        {
            var ver = (SetNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.Contains(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            var ver = (SetNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            ver.ExceptWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            var ver = (SetNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            ver.IntersectWith(other);
        }

        public bool Remove(T item)
        {
            var ver = (SetNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.Remove(item);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            var ver = (SetNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            ver.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            var ver = (SetNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            ver.UnionWith(other);
        }

        public IEqualityComparer<T> Comparer { get; }

        public int Count
        {
            get
            {
                var ver = (SetNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
                return ver.Count;
            }
        }

        protected override RevisionNode GetVersion(int id)
        {
            return GetVersion(id, root);
        }

        protected override RevisionNode CreateNode(int id, NodeType type, RevisionNode parent)
        {
            return Comparer == null
                ? new SetNode<T>(id, type, (SetNode<T>)parent)
                : new SetNode<T>(id, type, (SetNode<T>)parent, Comparer);
        }

        protected override void ApplyOperations(RevisionNode mergedVer, System.Collections.Generic.Stack<Operation> mergeOps, Func<T, T, T> mergeValueRule)
        {
            var ver = (SetNode<T>)mergedVer;

            while (mergeOps.Count > 0)
            {
                var op = mergeOps.Pop();
                switch (op.Type)
                {
                    case OperationType.Add:
                        ver.Add((T)op.Value);
                        break;

                    case OperationType.Remove:
                        ver.Remove((T)op.Value);
                        break;

                    case OperationType.Union:
                        ver.UnionWith((IEnumerable<T>)op.Value);
                        break;

                    case OperationType.Intersect:
                        ver.IntersectWith((IEnumerable<T>)op.Value);
                        break;

                    case OperationType.Difference:
                        ver.ExceptWith((IEnumerable<T>)op.Value);
                        break;

                    case OperationType.SymmetricDifference:
                        ver.SymmetricExceptWith((IEnumerable<T>)op.Value);
                        break;
                }
            }
        }
    }
}
