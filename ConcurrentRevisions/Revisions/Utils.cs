using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConcurrentRevisions
{
    public static class Utils
    {
        #region Default merge rules

        internal static System.Collections.Generic.Stack<Operation> SimpleMergeRule(System.Collections.Generic.Stack<Operation> main, System.Collections.Generic.Stack<Operation> join)
        {
            var newOps = new List<Operation>(join);
            newOps.AddRange(main);
            return new System.Collections.Generic.Stack<Operation>(newOps);
        }

        internal static System.Collections.Generic.Stack<Operation> SQMergeRule(System.Collections.Generic.Stack<Operation> main, System.Collections.Generic.Stack<Operation> join)
        {
            var newMain = new List<Operation>();

            foreach (var operation in main)
            {
                if (operation.Type == OperationType.Remove)
                {
                    var push = newMain.FirstOrDefault(op => op.Type == OperationType.Add && op.Value.Equals(operation.Value));
                    if (push != null)
                    {
                        newMain.Remove(push);
                        continue;
                    }
                }
                newMain.Add(operation);
            }

            var newJoin = new List<Operation>();

            foreach (var operation in join)
            {
                if (operation.Type == OperationType.Remove)
                {
                    var push = newJoin.FirstOrDefault(op => op.Type == OperationType.Add && op.Value.Equals(operation.Value));
                    if (push != null)
                    {
                        newJoin.Remove(push);
                        continue;
                    }
                }
                newJoin.Add(operation);
            }

            var addMain = newMain.Where(op => op.Type == OperationType.Add).ToList();
            var rmMain = newMain.Where(op => op.Type == OperationType.Remove).ToList();
            var addJoin = newJoin.Where(op => op.Type == OperationType.Add).ToList();
            var rmJoin = newJoin.Where(op => op.Type == OperationType.Remove).ToList();

            var res = new List<Operation>();
            if (rmMain.Count > rmJoin.Count)
                res.AddRange(rmMain);
            else
                res.AddRange(rmJoin);

            res.AddRange(addMain);
            res.AddRange(addJoin);

            res.Reverse();

            return new System.Collections.Generic.Stack<Operation>(res);
        }

        #endregion Default merge rules

        #region Extensions

        public static void Run(this Thread thread, params VersionedCollection[] collections)
        {
            foreach (var coll in collections)
                coll.Fork(thread.ManagedThreadId);

            thread.Start();
        }

        #endregion Extensions
    }
}
