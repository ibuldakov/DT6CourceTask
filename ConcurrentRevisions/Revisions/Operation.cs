using System;
using System.Threading;

namespace ConcurrentRevisions
{
    public enum OperationType
    {
        #region Common

        Add,
        Remove,

        #endregion Common

        #region Set

        Union,
        Intersect,
        Difference,
        SymmetricDifference,

        #endregion Set

        #region Tree

        Edit,

        #endregion Tree
    }

    public class Operation
    {
        private OperationType _type;
        private object _value;
        private int _threadId;
        private DateTime _time;

        public Operation(OperationType type, object value)
        {
            _type = type;
            _value = value;
            _threadId = Thread.CurrentThread.ManagedThreadId;
            _time = DateTime.Now;
        }

        public OperationType Type
        {
            get { return _type; }
        }

        public object Value
        {
            get { return _value; }
        }

        public DateTime Time
        {
            get { return _time; }
        }

        public override string ToString()
        {
            return _type.ToString() + ":" + _value.ToString();
        }
    }
}
