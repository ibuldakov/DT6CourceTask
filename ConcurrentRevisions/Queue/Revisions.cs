using System;
using System.Threading;

namespace ConcurrentRevisions
{
    internal class QueueRevisions<T> : Revisions<T>
    {
        private QueueNode<T> root;

        public QueueRevisions()
        {
            root = new QueueNode<T>(Thread.CurrentThread.ManagedThreadId);
        }

        public T Peek()
        {
            var ver = (QueueNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.Peek();
        }

        public T Dequeue()
        {
            var ver = (QueueNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            return ver.Dequeue();
        }


        public void Enqueue(T item)
        {
            var ver = (QueueNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
            ver.Enqueue(item);
        }

        public int Count
        {
            get
            {
                var ver = (QueueNode<T>)GetVersion(Thread.CurrentThread.ManagedThreadId);
                return ver.Count;
            }
        }

        protected override RevisionNode GetVersion(int id)
        {
            return GetVersion(id, root);
        }

        protected override RevisionNode CreateNode(int id, NodeType type, RevisionNode parent)
        {
            return new QueueNode<T>(id, type, (QueueNode<T>)parent);
        }

        protected override void ApplyOperations(RevisionNode mergedVer, System.Collections.Generic.Stack<Operation> mergeOps, Func<T, T, T> mergeValueRule)
        {
            var ver = (QueueNode<T>)mergedVer;

            while (mergeOps.Count > 0)
            {
                var op = mergeOps.Pop();
                switch (op.Type)
                {
                    case OperationType.Add:
                        ver.Enqueue((T)op.Value);
                        break;

                    case OperationType.Remove:
                        ver.Dequeue();
                        break;
                }
            }
        }
    }
}
