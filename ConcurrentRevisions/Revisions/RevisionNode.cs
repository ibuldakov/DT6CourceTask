using System.Collections.Generic;

namespace ConcurrentRevisions
{
    internal enum NodeType
    {
        Edit,
        Fork,
        Join,
        Root,
    }

    internal abstract class RevisionNode
    {
        #region Constructors

        public RevisionNode(int id)
        {
            ThreadId = id;
            Type = NodeType.Root;

            Operations = new System.Collections.Generic.Stack<Operation>();

            Parent = null;
            _children = new List<RevisionNode>();
        }

        public RevisionNode(int id, NodeType type, RevisionNode parent)
            : this(id)
        {
            Type = type;
            Parent = parent;
        }

        #endregion Constructors

        #region Public properties

        public int ThreadId { get; }

        public NodeType Type { get; set; }

        public RevisionNode Parent { get; set; }

        public System.Collections.Generic.Stack<Operation> Operations { get; }

        public IEnumerable<RevisionNode> Children { get { return _children; } }

        #endregion Public properties

        #region Public methods

        public void AddChild(params RevisionNode[] children)
        {
            _children.AddRange(children);
        }

        public void RemoveChild(RevisionNode child)
        {
            _children.Remove(child);
        }

        #endregion Public methods

        #region Private data

        private List<RevisionNode> _children;

        #endregion Private data
    }
}
