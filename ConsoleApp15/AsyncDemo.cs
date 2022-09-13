using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace AsyncDemoNS
{
    class AsyncDemo
    {
        private static readonly object consoleLock = new object();

        private static readonly CancellationTokenSource source = new CancellationTokenSource();
        private static readonly CancellationToken ct = source.Token;


        public async static Task<int> MocPrace()
        {
            int i = 0;
            await Task.Run(async () =>
            {
                for (i = 15; i > -1; i--)
                {
                    Debug.WriteLine($"MocPrace:{Thread.CurrentThread.ManagedThreadId}" +
                        $" IsBackground: {Thread.CurrentThread.IsBackground}", "SYNC");
                    await Task.Delay(300);
                    Console.SetCursorPosition(10, 10);
                    Console.Write(i);
                    Console.SetCursorPosition(0, 0);

                }
                // když skončíš, pošli CancellationRequest na všechny, kto mají token ze source
                source.Cancel();
            });
            return i;
        }

        public async static Task Vrtule(int x, int y)
        {
            async Task OverwriteVrtule(char c, int x, int y)
            {
                await Task.Delay(500);
                Console.SetCursorPosition(x, y);
                Console.Write(c);
            }

            await Task.Run(async () =>
            {
                Console.CursorVisible = false;

                while (true)
                {
                    if (ct.IsCancellationRequested && x > y)
                    {
                        await OverwriteVrtule ('*', x, y);
                        break;
                    }

                    Debug.WriteLine($"Vrtule[{x},{y}]: {Thread.CurrentThread.ManagedThreadId}" +
                        $" IsBackground: {Thread.CurrentThread.IsBackground}", "SYNC");
                    await OverwriteVrtule('|', x, y);
                    await OverwriteVrtule('/', x, y);
                    await OverwriteVrtule ('-', x, y);
                    await OverwriteVrtule ('\\', x, y);
                }
            });
        }

        static async Task Main(string[] args)
        {
            int availableWorkers = 0, availableAsyncIO = 0;
            ThreadPool.GetAvailableThreads(out availableWorkers, out availableAsyncIO);
            Console.WriteLine($"Available workers: {availableWorkers}, available Async: {availableAsyncIO}");

            Debug.WriteLine($"Main:{Thread.CurrentThread.ManagedThreadId}" +
                $" IsBackground: {Thread.CurrentThread.IsBackground}", "SYNC");

            // fire and forget - nečekej na výsledek
            MocPrace();

            Console.SetCursorPosition(0, 20);
            Console.WriteLine("Čekám na Enter");

            Task[] tasks =
            {
                Task.Run(()=>Vrtule(14, 14)),
                Task.Run(()=>Vrtule(14, 15)),
                Task.Run(()=>Vrtule(14, 16)),
                Task.Run(()=>Vrtule(15, 14)),
                Task.Run(()=>Vrtule(14, 15)),
                Task.Run(()=>Vrtule(15, 16)),
            };

            Task.WaitAll(tasks, ct);
            Console.ReadLine();
        }
    }
}
