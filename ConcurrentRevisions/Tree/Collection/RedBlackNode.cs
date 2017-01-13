using System;

namespace Collections
{
    internal class RedBlackNode<TKey, TValue>
        where TKey : IComparable, IComparable<TKey>, IEquatable<TKey>
    {
        public RedBlackNode()
        {
            Color = Color.Red;

            Right = Leaf;
            Left = Leaf;
        }

        public RedBlackNode(TKey key, TValue value)
            : this()
        {
            Key = key;
            Value = value;
        }


        public TKey Key { get; set; }

        public TValue Value { get; set; }


        internal Color Color { get; set; }

        public RedBlackNode<TKey, TValue> Parent { get; set; }

        public RedBlackNode<TKey, TValue> Left { get; set; }

        public RedBlackNode<TKey, TValue> Right { get; set; }

        internal static readonly RedBlackNode<TKey, TValue> Leaf =
            new RedBlackNode<TKey, TValue>
            {
                Left = null,
                Right = null,
                Parent = null,
                Color = Color.Black
            };
    }
}