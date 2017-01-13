using System.Linq;

namespace ConcurrentRevisions
{
    internal class StackNode<T> : RevisionNode
    {
        public StackNode(int id)
            : base(id)
        {
            _initial = new System.Collections.Generic.Stack<T>();
            Current = new System.Collections.Generic.Stack<T>();
        }

        public StackNode(int id, NodeType type, StackNode<T> parent)
            : base(id, type, parent)
        {
            var init = parent.Current.Reverse();
            _initial = new System.Collections.Generic.Stack<T>(init);
            Current = new System.Collections.Generic.Stack<T>(init);
        }

        public void Push(T item)
        {
            Current.Push(item);
            Operations.Push(new Operation(OperationType.Add, item));
        }

        public T Pop()
        {
            T item = Current.Pop();
            Operations.Push(new Operation(OperationType.Remove, item));
            return item;
        }

        public T Peek()
        {
            return Current.Peek();
        }

        public int Count
        {
            get { return Current.Count; }
        }

        private System.Collections.Generic.Stack<T> _initial;
        internal System.Collections.Generic.Stack<T> Current;
    }
}
