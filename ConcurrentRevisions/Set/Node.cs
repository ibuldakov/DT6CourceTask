using System.Collections.Generic;
using System.Linq;

namespace ConcurrentRevisions
{
    internal class SetNode<T> : RevisionNode
    {
        public SetNode(int id)
            : base(id)
        {
            _initial = new HashSet<T>();
            _current = new HashSet<T>();
        }

        public SetNode(int id, IEqualityComparer<T> comparer)
            : base(id)
        {
            _initial = new HashSet<T>(comparer);
            _current = new HashSet<T>(comparer);
        }

        public SetNode(int id, NodeType type, SetNode<T> parent)
            : base(id, type, parent)
        {
            var init = parent._current.Reverse();
            _initial = new HashSet<T>(init);
            _current = new HashSet<T>(init);
        }

        public SetNode(int id, NodeType type, SetNode<T> parent, IEqualityComparer<T> comparer)
            : base(id, type, parent)
        {
            var init = parent._current.Reverse();
            _initial = new HashSet<T>(init, comparer);
            _current = new HashSet<T>(init, comparer);
        }

        public bool Add(T item)
        {
            var res = _current.Add(item);
            Operations.Push(new Operation(OperationType.Add, item));
            return res;
        }

        public bool Contains(T item)
        {
            return _current.Contains(item);
        }

        public bool Remove(T item)
        {
            var res = _current.Remove(item);
            Operations.Push(new Operation(OperationType.Remove, item));
            return res;
        }

        public void UnionWith(IEnumerable<T> other)
        {
            _current.UnionWith(other);
            Operations.Push(new Operation(OperationType.Union, other));
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            _current.IntersectWith(other);
            Operations.Push(new Operation(OperationType.Intersect, other));
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            _current.ExceptWith(other);
            Operations.Push(new Operation(OperationType.Difference, other));
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            _current.SymmetricExceptWith(other);
            Operations.Push(new Operation(OperationType.SymmetricDifference, other));
        }

        public int Count
        {
            get { return _current.Count; }
        }

        private HashSet<T> _initial;
        private HashSet<T> _current;
    }
}
