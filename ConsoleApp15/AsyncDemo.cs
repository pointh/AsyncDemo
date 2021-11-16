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
            await Task.Run(() =>
            {
                for (i = 15; i > -1; i--)
                {
                    Debug.WriteLine($"MocPrace:{Thread.CurrentThread.ManagedThreadId}" + 
                        $" IsBackground: {Thread.CurrentThread.IsBackground}", "SYNC");
                    Thread.Sleep(300);
                    lock (consoleLock)
                    {
                        Console.SetCursorPosition(10, 10);
                        Console.Write(i);
                        Console.SetCursorPosition(0, 0);
                    }
                }
                // když skončíš, pošli CancellationRequest na všechny, kto mají token ze source
                source.Cancel();
            });
            return i;
        }

        public async static Task Vrtule(int x, int y)
        {
            void OverwriteVrtule(char c)
            {
                lock (consoleLock)
                {
                    Thread.Sleep(10);
                    Console.SetCursorPosition(x, y);
                    Console.Write(c);
                }
            }

            await Task.Run(() =>
            {
                Console.CursorVisible = false;

                while (true)
                {
                    if (ct.IsCancellationRequested && x > y)
                    {
                        OverwriteVrtule('*');
                        break;
                    }

                    Debug.WriteLine($"Vrtule[{x},{y}]: {Thread.CurrentThread.ManagedThreadId}" +
                        $" IsBackground: {Thread.CurrentThread.IsBackground}", "SYNC");
                    OverwriteVrtule('|');
                    OverwriteVrtule('/');
                    OverwriteVrtule('-');
                    OverwriteVrtule('\\');
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

            for (int k = 15; k <= 16; k++)
            {
                for (int j = 15; j <= 16; j++)
                {
                    // fire and forget - nečekej na výsledek
                    // Vrtule se naplánují v thread poolu, o spouštění si rozhodne OS
                    Thread t = new Thread(async () => await Vrtule (k, j));
                    t.IsBackground = true;
                    t.Name = $"{k}:{j}";
                    t.Priority = ThreadPriority.AboveNormal;
                    t.Start();
                }
            }

            // tohle se vypíše hned, pokud kdykoliv přijde Enter, aplikace skončí
            
            Console.ReadLine();
        }
    }
}
