using System;
using System.Collections.Generic;

namespace ConcurrentRevisions
{
    internal class RBTreeNode<TKey, TValue> : RevisionNode
        where TKey : IComparable<TKey>, IComparable, IEquatable<TKey>
    {
        public RBTreeNode(int id)
            : base(id)
        {
            _initial = new Collections.RedBlackTree<TKey, TValue>();
            _current = new Collections.RedBlackTree<TKey, TValue>();
        }

        public RBTreeNode(int id, NodeType type, RBTreeNode<TKey, TValue> parent)
            : base(id, type, parent)
        {
            var init = parent._current;
            _initial = new Collections.RedBlackTree<TKey, TValue>(init);
            _current = new Collections.RedBlackTree<TKey, TValue>(init);
        }

        public void Add(TKey key, TValue value)
        {
            _current.Add(key, value);
            Operations.Push(new Operation(OperationType.Edit, new Tuple<TKey, TValue, TValue>(key, default(TValue), value)));
        }

        public bool Remove(TKey key)
        {
            var res = _current.Remove(key);
            Operations.Push(new Operation(OperationType.Remove, key));
            return res;
        }

        public bool ContainsKey(TKey key)
        {
            return _current.ContainsKey(key);
        }

        public TValue this[TKey key]
        {
            get
            {
                return ((IDictionary<TKey, TValue>)_current)[key];
            }
            set
            {
                var dict = ((IDictionary<TKey, TValue>)_current);
                var oldValue = dict[key];
                dict[key] = value;
                Operations.Push(new Operation(OperationType.Edit, new Tuple<TKey, TValue, TValue>(key, oldValue, value)));
            }
        }

        public int Count
        {
            get { return _current.Count; }
        }

        public ICollection<TKey> Keys
        {
            get { return _current.Keys; }
        }

        public ICollection<TValue> Values
        {
            get { return _current.Values; }
        }

        private Collections.RedBlackTree<TKey, TValue> _initial;
        private Collections.RedBlackTree<TKey, TValue> _current;
    }
}
