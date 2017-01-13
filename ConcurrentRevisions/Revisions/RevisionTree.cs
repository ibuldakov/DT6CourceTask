using System;
using System.Linq;
using System.Threading;

namespace ConcurrentRevisions
{
    internal abstract class Revisions<T>
    {
        private readonly object lockObject = new object();

        public void Fork(int id)
        {
            lock (lockObject)
            {
                var current = Thread.CurrentThread.ManagedThreadId;
                var ver = GetVersion(current);

                ver.AddChild(CreateNode(current, NodeType.Fork, ver), CreateNode(id, NodeType.Fork, ver));
            }
        }

        public bool CanJoin(int main, int join)
        {
            lock (lockObject)
            {
                var joinVer = GetVersion(join);
                var fork = GetFork(joinVer);
                return GetVersion(main, fork) != null;
            }
        }

        public void Join(int main, int join, Func<System.Collections.Generic.Stack<Operation>, System.Collections.Generic.Stack<Operation>, System.Collections.Generic.Stack<Operation>> mergeVersionRule, Func<T, T, T> mergeValueRule)
        {
            lock (lockObject)
            {
                var mainVer = GetVersion(main);
                var joinVer = GetVersion(join);

                var fork = GetFork(joinVer);

                if (GetVersion(main, fork) == null)
                    throw new Exception($"Unable to merge versions {main} and {join}");

                var initForMerge = GetInitForMerge(mainVer, fork);

                var mergedVer = CreateNode(main, NodeType.Join, initForMerge);

                var mainOps = GetOperations(mainVer, initForMerge);
                var joinOps = GetOperations(joinVer, fork);

                var mergeOps = mergeVersionRule(mainOps, joinOps);
                ApplyOperations(mergedVer, mergeOps, mergeValueRule);

                var joinStart = fork.Children.First(v => v.ThreadId == join);
                fork.RemoveChild(joinStart);
                mainVer.AddChild(joinStart);
                joinStart.Parent = mainVer;

                joinVer.AddChild(mergedVer);
                mergedVer.Parent = joinVer;

                mergedVer.AddChild(CreateNode(main, NodeType.Edit, mergedVer));
            }
        }

        protected abstract RevisionNode GetVersion(int id);

        protected abstract RevisionNode CreateNode(int id, NodeType type, RevisionNode parent);

        protected abstract void ApplyOperations(RevisionNode mergedVer, System.Collections.Generic.Stack<Operation> mergeOps, Func<T, T, T> mergeValueRule);

        protected RevisionNode GetVersion(int id, RevisionNode root)
        {
            if (!root.Children.Any())
                return root.ThreadId == id ? root : null;

            return root.Children.Select(node => GetVersion(id, node)).FirstOrDefault(node => node != null);
        }

        private RevisionNode GetFork(RevisionNode ver)
        {
            var id = ver.ThreadId;

            while (ver.Type != NodeType.Fork || ver.Parent.ThreadId == id)
                ver = ver.Parent;

            return ver.Parent;
        }

        private RevisionNode GetInitForMerge(RevisionNode ver, RevisionNode from)
        {
            while (ver != from && ver.Type != NodeType.Fork && ver.Type != NodeType.Join)
                ver = ver.Parent;

            return ver;
        }

        private System.Collections.Generic.Stack<Operation> GetOperations(RevisionNode ver, RevisionNode from)
        {
            var ops = new System.Collections.Generic.Stack<Operation>();

            while (ver != from)
            {
                while (ver.Operations.Count > 0)
                    ops.Push(ver.Operations.Pop());

                ver = ver.Parent;
            }

            return ops;
        }
    }
}
