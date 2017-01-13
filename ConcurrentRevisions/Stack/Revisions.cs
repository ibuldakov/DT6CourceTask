using System;
using System.Threading;

namespace ConcurrentRevisions
{
    internal class StackRevisions<T> : Revisions<T>
    {
        private StackNode<T> root;

        public StackRevisions()
        {
            root = new StackNode<T>(Thread.CurrentThread.ManagedThreadId);
        }

        public T Peek()
        {
            var ver = (StackNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.Peek();
        }

        public T Pop()
        {
            var ver = (StackNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.Pop();
        }


        public void Push(T item)
        {
            var ver = (StackNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            ver.Push(item);
        }

        public int Count
        {
            get
            {
                var ver = (StackNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
                return ver.Count;
            }
        }

        protected override RevisionNode GetVersion(int id)
        {
            return GetVersion(id, root);
        }

        protected override RevisionNode CreateNode(int id, NodeType type, RevisionNode parent)
        {
            return new StackNode<T>(id, type, (StackNode<T>)parent);
        }

        protected override void ApplyOperations(RevisionNode mergedVer, System.Collections.Generic.Stack<Operation> mergeOps, Func<T, T, T> mergeValueRule)
        {
            var ver = (StackNode<T>)mergedVer;

            while (mergeOps.Count > 0)
            {
                var op = mergeOps.Pop();
                switch (op.Type)
                {
                    case OperationType.Add:
                        ver.Push((T)op.Value);
                        break;

                    case OperationType.Remove:
                        ver.Pop();
                        break;
                }
            }
        }
    }
}
