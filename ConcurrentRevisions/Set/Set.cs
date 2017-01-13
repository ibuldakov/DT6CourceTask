using System;
using System.Collections.Generic;
using System.Threading;

namespace ConcurrentRevisions
{
    public class Set<T> : VersionedCollection<T>
    {
        public Set()
        {
            revisions = new SetRevisions<T>();
        }

        public Set(IEqualityComparer<T> comparer)
        {
            revisions = new SetRevisions<T>(comparer);
        }

        public IEqualityComparer<T> Comparer
        {
            get
            {
                return revisions.Comparer;
            }
        }

        public override int Count
        {
            get
            {
                return revisions.Count;
            }
        }

        public bool Add(T item)
        {
            return revisions.Add(item);
        }

        public bool Contains(T item)
        {
            return revisions.Contains(item);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            revisions.ExceptWith(other);
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            revisions.IntersectWith(other);
        }

        public bool Remove(T item)
        {
            return revisions.Remove(item);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            revisions.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            revisions.UnionWith(other);
        }

        internal override void Fork(int id)
        {
            revisions.Fork(id);
        }

        public override bool CanJoin(int id)
        {
            return revisions.CanJoin(Thread.CurrentThread.ManagedThreadId, id);
        }

        public override void Join(int id, Func<System.Collections.Generic.Stack<Operation>, System.Collections.Generic.Stack<Operation>, System.Collections.Generic.Stack<Operation>> mergeRule, Func<T, T, T> mergeValueRule)
        {
            revisions.Join(Thread.CurrentThread.ManagedThreadId, id, mergeRule ?? Utils.SimpleMergeRule, mergeValueRule);
        }

        private SetRevisions<T> revisions;
    }
}
