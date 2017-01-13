using System;
using System.Threading;

namespace ConcurrentRevisions
{
    public class Queue<T> : VersionedCollection<T>
    {
        public Queue()
        {
            revisions = new QueueRevisions<T>();
        }

        public override int Count
        {
            get
            {
                return revisions.Count;
            }
        }

        public T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("The Queue is empty");

            return revisions.Dequeue();
        }

        public void Enqueue(T item)
        {
            revisions.Enqueue(item);
        }

        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("The Queue is empty");

            return revisions.Peek();
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
            revisions.Join(Thread.CurrentThread.ManagedThreadId, id, mergeRule ?? Utils.SQMergeRule, null);
        }

        private QueueRevisions<T> revisions;
    }
}
