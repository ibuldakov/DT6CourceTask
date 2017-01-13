using System;

namespace ConcurrentRevisions
{
    public abstract class VersionedCollection
    {
        public abstract int Count { get; }

        internal abstract void Fork(int id);

        public abstract bool CanJoin(int id);
    }

    public abstract class VersionedCollection<T> : VersionedCollection
    {
        public abstract void Join(int id, Func<System.Collections.Generic.Stack<Operation>, System.Collections.Generic.Stack<Operation>, System.Collections.Generic.Stack<Operation>> mergeVersionRule, Func<T, T, T> mergeValueRule);
    }
}
