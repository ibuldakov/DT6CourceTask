using System;
using System.Threading;

namespace ConcurrentRevisions
{
    public class Stack<T> : VersionedCollection<T>
    {
        public Stack()
        {
            revisions = new StackRevisions<T>();
        }

        public override int Count
        {
            get
            {
                return revisions.Count;
            }
        }

        public T Peek()
        {
            if (Count == 0)
                throw new InvalidOperationException("The Stack is empty");

            return revisions.Peek();
        }

        public T Pop()
        {
            if (Count == 0)
                throw new InvalidOperationException("The Stack is empty");

            return revisions.Pop();
        }

        public void Push(T item)
        {
            revisions.Push(item);
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
            revisions.Join(Thread.CurrentThread.ManagedThreadId, id, mergeRule ?? Utils.SQMergeRule, mergeValueRule);
        }

        private StackRevisions<T> revisions;
    }
}
