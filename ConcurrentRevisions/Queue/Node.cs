namespace ConcurrentRevisions
{
    internal class QueueNode<T> : RevisionNode
    {
        public QueueNode(int id)
            : base(id)
        {
            _initial = new System.Collections.Generic.Queue<T>();
            _current = new System.Collections.Generic.Queue<T>();
        }

        public QueueNode(int id, NodeType type, QueueNode<T> parent)
            : base(id, type, parent)
        {
            var init = parent._current;
            _initial = new System.Collections.Generic.Queue<T>(init);
            _current = new System.Collections.Generic.Queue<T>(init);
        }

        public void Enqueue(T item)
        {
            _current.Enqueue(item);
            Operations.Push(new Operation(OperationType.Add, item));
        }

        public T Dequeue()
        {
            T item = _current.Dequeue();
            Operations.Push(new Operation(OperationType.Remove, item));
            return item;
        }

        public T Peek()
        {
            return _current.Peek();
        }

        public int Count
        {
            get { return _current.Count; }
        }

        private System.Collections.Generic.Queue<T> _initial;
        private System.Collections.Generic.Queue<T> _current;
    }
}
