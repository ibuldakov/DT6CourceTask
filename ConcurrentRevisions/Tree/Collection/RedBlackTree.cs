using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Collections
{
    public class RedBlackTree<TKey, TValue> 
        : IDictionary<TKey, TValue>
        where TKey : IComparable<TKey>, IComparable, IEquatable<TKey>
    {
        private RedBlackNode<TKey, TValue> _root = RedBlackNode<TKey, TValue>.Leaf;
        private int _count;

        private RedBlackNode<TKey, TValue> _lastNodeFound = RedBlackNode<TKey, TValue>.Leaf;

        public RedBlackTree()
        { }

        public RedBlackTree(IDictionary<TKey, TValue> collection)
        {
            foreach (var item in collection)
                New(item.Key, item.Value);
        }

        public int Count { get { return _count; } }

        public bool IsReadOnly { get { return false; } }

        public virtual void Add(TKey key, TValue value)
        {
            New(key, value);
        }

        public virtual bool Remove(TKey key)
        {
            try
            {
                Delete(GetNode(key));
                return true;
            }
            catch (Exception)
            { }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            try
            {
                var node = GetNode(key);
                return node != null;
            }
            catch
            { }

            return false;
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get { return GetNode(key).Value; }
            set { GetNode(key).Value = value; }
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            New(item.Key, item.Value);
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return GetAll().Select(i => i.Value).GetEnumerator();
        }

        public bool IsEmpty()
        {
            return _root == RedBlackNode<TKey, TValue>.Leaf;
        }

        public void Clear()
        {
            _root = RedBlackNode<TKey, TValue>.Leaf;
            _count = 0;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "ArrayIndex cannot be less than 0");
            if (array == null)
                throw new ArgumentNullException("array");
            if ((array.Length - arrayIndex) < Count)
                throw new ArgumentException();

            int currentPosition = arrayIndex;
            foreach (TValue item in GetAll().Select(i => i.Value))
            {
                array.SetValue(item, currentPosition);
                currentPosition++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetAll().GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetAll()
                .Select(i => new KeyValuePair<TKey, TValue>(i.Key, i.Value))
                .GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = GetNode(key).Value;
            return value != null;
        }

        public ICollection<TKey> Keys
        {
            get { return GetAll().Select(i => i.Key).ToArray(); }
        }

        public ICollection<TValue> Values
        {
            get { return GetAll().Select(i => i.Value).ToArray(); }
        }

        #region "Private Methods"

        private void New(TKey key, TValue data)
        {
            RedBlackNode<TKey, TValue> newNode = new RedBlackNode<TKey, TValue>(key, data);

            RedBlackNode<TKey, TValue> workNode = _root;

            while (workNode != RedBlackNode<TKey, TValue>.Leaf)
            {
                // find Parent
                newNode.Parent = workNode;
                int result = key.CompareTo(workNode.Key);
                if (result == 0)
                    throw (new InvalidOperationException("Key already exist"));

                workNode = result > 0
                    ? workNode.Right
                    : workNode.Left;
            }

            if (newNode.Parent != null)
            {
                if (newNode.Key.CompareTo(newNode.Parent.Key) > 0)
                    newNode.Parent.Right = newNode;
                else
                    newNode.Parent.Left = newNode;
            }
            else
            {
                _root = newNode;
            }

            BalanceTreeAfterInsert(newNode);

            _lastNodeFound = newNode;

            Interlocked.Increment(ref _count);
        }

        private void Delete(RedBlackNode<TKey, TValue> deleteNode)
        {
            RedBlackNode<TKey, TValue> workNode;

            if (deleteNode.Left == RedBlackNode<TKey, TValue>.Leaf || deleteNode.Right == RedBlackNode<TKey, TValue>.Leaf)
                workNode = deleteNode;
            else
            {
                workNode = deleteNode.Right;
                while (workNode.Left != RedBlackNode<TKey, TValue>.Leaf)
                    workNode = workNode.Left;
            }

            RedBlackNode<TKey, TValue> linkedNode = workNode.Left != RedBlackNode<TKey, TValue>.Leaf
                                                 ? workNode.Left
                                                 : workNode.Right;

            linkedNode.Parent = workNode.Parent;
            if (workNode.Parent != null)
                if (workNode == workNode.Parent.Left)
                    workNode.Parent.Left = linkedNode;
                else
                    workNode.Parent.Right = linkedNode;
            else
                _root = linkedNode;

            if (workNode != deleteNode)
            {
                deleteNode.Key = workNode.Key;
                deleteNode.Value = workNode.Value;
            }

            if (workNode.Color == Color.Black)
                BalanceTreeAfterDelete(linkedNode);

            _lastNodeFound = RedBlackNode<TKey, TValue>.Leaf;

            Interlocked.Decrement(ref _count);
        }

        private void BalanceTreeAfterDelete(RedBlackNode<TKey, TValue> linkedNode)
        {
            while (linkedNode != _root && linkedNode.Color == Color.Black)
            {
                RedBlackNode<TKey, TValue> workNode;
                if (linkedNode == linkedNode.Parent.Left)
                {
                    workNode = linkedNode.Parent.Right;
                    if (workNode.Color == Color.Red)
                    {
                        linkedNode.Parent.Color = Color.Red;
                        workNode.Color = Color.Black;
                        RotateLeft(linkedNode.Parent);
                        workNode = linkedNode.Parent.Right;
                    }

                    if (workNode.Left.Color == Color.Black && workNode.Right.Color == Color.Black)
                    {
                        workNode.Color = Color.Red;
                        linkedNode = linkedNode.Parent;
                    }
                    else
                    {
                        if (workNode.Right.Color == Color.Black)
                        {
                            workNode.Left.Color = Color.Black;
                            workNode.Color = Color.Red;
                            RotateRight(workNode);
                            workNode = linkedNode.Parent.Right;
                        }
                        linkedNode.Parent.Color = Color.Black;
                        workNode.Color = linkedNode.Parent.Color;
                        workNode.Right.Color = Color.Black;
                        RotateLeft(linkedNode.Parent);
                        linkedNode = _root;
                    }
                }
                else
                { 
                    workNode = linkedNode.Parent.Left;
                    if (workNode.Color == Color.Red)
                    {
                        linkedNode.Parent.Color = Color.Red;
                        workNode.Color = Color.Black;
                        RotateRight(linkedNode.Parent);
                        workNode = linkedNode.Parent.Left;
                    }
                    if (workNode.Right.Color == Color.Black &&
                        workNode.Left.Color == Color.Black)
                    {
                        workNode.Color = Color.Red;
                        linkedNode = linkedNode.Parent;
                    }
                    else
                    {
                        if (workNode.Left.Color == Color.Black)
                        {
                            workNode.Right.Color = Color.Black;
                            workNode.Color = Color.Red;
                            RotateLeft(workNode);
                            workNode = linkedNode.Parent.Left;
                        }
                        workNode.Color = linkedNode.Parent.Color;
                        linkedNode.Parent.Color = Color.Black;
                        workNode.Left.Color = Color.Black;
                        RotateRight(linkedNode.Parent);
                        linkedNode = _root;
                    }
                }
            }
            linkedNode.Color = Color.Black;
        }

        internal Stack<RedBlackNode<TKey, TValue>> GetAll()
        {
            var stack = new Stack<RedBlackNode<TKey, TValue>>();

            if (_root != RedBlackNode<TKey, TValue>.Leaf)
                WalkNextLevel(_root, stack);

            return stack;
        }

        private static void WalkNextLevel(RedBlackNode<TKey, TValue> node, Stack<RedBlackNode<TKey, TValue>> stack)
        {
            if (node.Right != RedBlackNode<TKey, TValue>.Leaf)
                WalkNextLevel(node.Right, stack);

            stack.Push(node);

            if (node.Left != RedBlackNode<TKey, TValue>.Leaf)
                WalkNextLevel(node.Left, stack);
        }

        private RedBlackNode<TKey, TValue> GetNode(TKey key)
        {
            int result;
            if (_lastNodeFound != RedBlackNode<TKey, TValue>.Leaf)
            {
                result = key.CompareTo(_lastNodeFound.Key);
                if (result == 0)
                    return _lastNodeFound;
            }

            RedBlackNode<TKey, TValue> treeNode = _root;

            while (treeNode != RedBlackNode<TKey, TValue>.Leaf)
            {
                result = key.CompareTo(treeNode.Key);
                if (result == 0)
                {
                    _lastNodeFound = treeNode;
                    return treeNode;
                }
                treeNode = result < 0
                    ? treeNode.Left
                    : treeNode.Right;
            }
            return null;
        }

        private void RotateRight(RedBlackNode<TKey, TValue> rotateNode)
        {
            var workNode = rotateNode.Left;

            rotateNode.Left = workNode.Right;

            if (workNode.Right != RedBlackNode<TKey, TValue>.Leaf)
                workNode.Right.Parent = rotateNode;

            if (workNode != RedBlackNode<TKey, TValue>.Leaf)
                workNode.Parent = rotateNode.Parent;

            if (rotateNode.Parent != null)
            {
                if (rotateNode == rotateNode.Parent.Right)
                    rotateNode.Parent.Right = workNode;
                else
                    rotateNode.Parent.Left = workNode;
            }
            else
                _root = workNode;

            workNode.Right = rotateNode;
            if (rotateNode != RedBlackNode<TKey, TValue>.Leaf)
                rotateNode.Parent = workNode;
        }

        private void RotateLeft(RedBlackNode<TKey, TValue> rotateNode)
        {
            RedBlackNode<TKey, TValue> workNode = rotateNode.Right;

            rotateNode.Right = workNode.Left;

            if (workNode.Left != RedBlackNode<TKey, TValue>.Leaf)
                workNode.Left.Parent = rotateNode;

            if (workNode != RedBlackNode<TKey, TValue>.Leaf)
                workNode.Parent = rotateNode.Parent;

            if (rotateNode.Parent != null)
            {
                if (rotateNode == rotateNode.Parent.Left)
                    rotateNode.Parent.Left = workNode;
                else
                    rotateNode.Parent.Right = workNode;
            }
            else
                _root = workNode;

            workNode.Left = rotateNode;
            if (rotateNode != RedBlackNode<TKey, TValue>.Leaf)
                rotateNode.Parent = workNode;
        }

        private void BalanceTreeAfterInsert(RedBlackNode<TKey, TValue> insertedNode)
        {
            while (insertedNode != _root && insertedNode.Parent.Color == Color.Red)
            {
                RedBlackNode<TKey, TValue> workNode;
                if (insertedNode.Parent == insertedNode.Parent.Parent.Left)
                { 
                    workNode = insertedNode.Parent.Parent.Right;
                    if (workNode != null && workNode.Color == Color.Red)
                    {	
                        insertedNode.Parent.Color = Color.Black;
                        workNode.Color = Color.Black;
                        insertedNode.Parent.Parent.Color = Color.Red;
                        insertedNode = insertedNode.Parent.Parent;
                    }
                    else
                    {
                        if (insertedNode == insertedNode.Parent.Right)
                        {
                            insertedNode = insertedNode.Parent;
                            RotateLeft(insertedNode);
                        }
                        insertedNode.Parent.Color = Color.Black;
                        insertedNode.Parent.Parent.Color = Color.Red;
                        RotateRight(insertedNode.Parent.Parent);
                    }
                }
                else
                {
                    workNode = insertedNode.Parent.Parent.Left;
                    if (workNode != null && workNode.Color == Color.Red)
                    {
                        insertedNode.Parent.Color = Color.Black;
                        workNode.Color = Color.Black;
                        insertedNode.Parent.Parent.Color = Color.Red;
                        insertedNode = insertedNode.Parent.Parent;
                    }
                    else
                    {
                        if (insertedNode == insertedNode.Parent.Left)
                        {
                            insertedNode = insertedNode.Parent;
                            RotateRight(insertedNode);
                        }
                        insertedNode.Parent.Color = Color.Black;
                        insertedNode.Parent.Parent.Color = Color.Red;
                        RotateLeft(insertedNode.Parent.Parent);
                    }
                }
            }
            _root.Color = Color.Black;
        }

        #endregion
    }
}