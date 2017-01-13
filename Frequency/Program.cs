using ConcurrentRevisions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Frequency
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                var rbtree = new RedBlackTree<char, int>();

                var text = File.ReadAllText("..\\..\\text.txt");
                var textLen = text.Length;
                var partLen = textLen / 3;
                var parts = new[] { text.Substring(0, partLen), text.Substring(partLen, partLen), text.Substring(2 * partLen) };

                var threads = Enumerable.Range(0, 3).Select(idx => new Thread(() =>
                {
                    var txt = parts[idx];

                    foreach (var ch in txt)
                    {
                        if (!rbtree.ContainsKey(ch))
                            rbtree.Add(ch, 0);

                        rbtree[ch] += 1;
                    }
                })).ToArray();

                foreach (var t in threads)
                    t.Run(rbtree);

                foreach (var t in threads)
                    t.Join();

                foreach (var t in threads)
                    rbtree.Join(t.ManagedThreadId, FreqTreeMergeRule, MergeIntegers);

                var charsBase = new[] { ' ', '+', '-', '.', 'a', 'b', 'c', 'z' };
                var freqBase = new[] { 3, 2, 2, 2, 20, 30, 40, 3 };

                var charsTest = rbtree.Keys.ToArray();
                var freqTest = rbtree.Values.ToArray();

                for (int i = 0; i < 8; ++i)
                {
                    if (charsBase[i] != charsTest[i] || freqTest[i] != freqBase[i])
                        Console.WriteLine($"Invalid value: base - [{charsBase[i]} => {freqBase[i]}] test - [{charsTest[i]} => {freqTest[i]}]");
                }

                Console.WriteLine("Passed");
                Console.ReadKey();
            }
            catch
            { }
        }

        #region Merge Rules

        private static System.Collections.Generic.Stack<Operation> FreqTreeMergeRule(System.Collections.Generic.Stack<Operation> main, System.Collections.Generic.Stack<Operation> join)
        {
            var rm = main.Where(op => op.Type == OperationType.Remove)
                .Union(join.Where(op => op.Type == OperationType.Remove));

            var newMain = main.Where(op => !rm.Any(rmOp => rmOp.Value.Equals(op.Value)));
            var newJoin = join.Where(op => !rm.Any(rmOp => rmOp.Value.Equals(op.Value)));

            var editMain = newMain.Where(op => op.Type == OperationType.Edit).GroupBy(op => ((Tuple<char, int, int>)op.Value).Item1);
            var editJoin = newJoin.Where(op => op.Type == OperationType.Edit).GroupBy(op => ((Tuple<char, int, int>)op.Value).Item1);

            Dictionary<char, Operation> ops = new Dictionary<char, Operation>();
            foreach (var group in editMain)
            {
                var before = ((Tuple<char, int, int>)group.First().Value).Item2;
                var after = ((Tuple<char, int, int>)group.Last().Value).Item3;

                ops.Add(group.Key, new Operation(OperationType.Edit, new Tuple<char, int, int>(group.Key, before, after)));
            }

            foreach (var group in editJoin)
            {
                var before = ((Tuple<char, int, int>)group.First().Value).Item2;
                var after = ((Tuple<char, int, int>)group.Last().Value).Item3;

                if (ops.ContainsKey(group.Key))
                {
                    var oldAfter = ((Tuple<char, int, int>)ops[group.Key].Value).Item3;
                    after += oldAfter;
                    ops[group.Key] = new Operation(OperationType.Edit, new Tuple<char, int, int>(group.Key, before, after));
                }
                else
                    ops.Add(group.Key, new Operation(OperationType.Edit, new Tuple<char, int, int>(group.Key, before, after)));
            }

            var init = Enumerable.Union(ops.Values, rm);

            return new System.Collections.Generic.Stack<Operation>(init);
        }

        private static int MergeIntegers(int x, int y)
        {
            return x + y;
        }

        #endregion Merge Rules
    }
}
