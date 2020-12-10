using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

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
                    Thread.Sleep(100);
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


            Debug.WriteLine($"Main:{Thread.CurrentThread.ManagedThreadId}" +
                $" IsBackground: {Thread.CurrentThread.IsBackground}", "SYNC");

            // normálně je MaxThreads = 4 - asi podle procesorů
            string v = $"ThereadPool setting : {ThreadPool.SetMaxThreads(72, 72)}";
            Console.WriteLine(v);

            // fire and forget - nečekej na výsledek
            MocPrace();

            for (int k = 16; k <= 21; k++)
            {
                for (int j = 16; j <= 21; j++)
                {
                    // fire and forget - nečekej na výsledek
                    // Vrtule se naplánují v thread poolu, o spouštění si rozhodne OS
                    Vrtule(k, j);
                }
            }

            // tohle se vypíše hned, pokud kdykoliv přijde Enter, aplikace skončí
            Console.WriteLine("Čekám na Enter");
            Console.ReadLine();
        }
    }
}
