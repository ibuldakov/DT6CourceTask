using ConcurrentRevisions;
using System;
using System.Linq;
using System.Threading;

namespace RandomTimes
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var stack = new Stack<int>();
                var queue = new Queue<int>();

                var threads = Enumerable.Range(0, 10).Select(idx => new Thread(() =>
                {
                    var rnd = new Random();
                    var num = idx * 10;
                    for (int i = 0; i < 10; ++i)
                    {
                        Thread.Sleep(rnd.Next(500, 5000));
                        stack.Push(num);
                        queue.Enqueue(num);
                        num++;
                    }
                })).ToArray();

                foreach (var t in threads)
                    t.Run(stack, queue);

                foreach (var t in threads)
                    t.Join();

                foreach (var t in threads)
                {
                    stack.Join(t.ManagedThreadId, null, null);
                    queue.Join(t.ManagedThreadId, null, null);
                }

                if (stack.Count != 100)
                    Console.WriteLine($"Invalid number of items in Stack [base: {100}; test: {stack.Count}]");

                if (queue.Count != 100)
                    Console.WriteLine($"Invalid number of items in Queue [base: {100}; test: {stack.Count}]");

                int _test = -1;
                int _base = 99;
                while (stack.Count > 0)
                {
                    _test = stack.Pop();
                    if (_test != _base)
                        Console.WriteLine($"Invalid item in Stack [base: {_base}; test: {_test}]");
                    _base--;
                }

                _test = -1;
                _base = 0;
                while (queue.Count > 0)
                {
                    _test = queue.Dequeue();
                    if (_test != _base)
                        Console.WriteLine($"Invalid item in Queue [base: {_base}; test: {_test}]");
                    _base++;
                }

                Console.WriteLine("Passed");
                Console.ReadKey();
            }
            catch
            { }
        }
    }
}
