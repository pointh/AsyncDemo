using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AsyncDemoNS
{
    class AsyncDemo
    {
        private static readonly object consoleLock = new object();

        public async static Task<int> MocPrace()
        {
            int i = 0;
            await Task.Run(() =>
            {
                for (i = 0; i < 20; i++)
                {
                    Debug.WriteLine($"MocPrace:{System.Threading.Thread.CurrentThread.ManagedThreadId}", "SYNC");
                    System.Threading.Thread.Sleep(1000);
                    Console.SetCursorPosition(10, 10);
                    Console.Write(i);
                    Console.SetCursorPosition(0, 0);
                }
            });
            return i;
        }

        public async static Task Vrtule(int x, int y)
        {
            void OverwriteOn0(char c)
            {
                lock (consoleLock)
                {
                    System.Threading.Thread.Sleep(50);
                    Console.SetCursorPosition(x, y);
                    Console.Write(c);
                }
            }

            await Task.Run(() =>
            {
                Console.CursorVisible = false;

                while (true)
                {
                    Debug.WriteLine($"Vrtule[{x},{y}]: {System.Threading.Thread.CurrentThread.ManagedThreadId}", "SYNC");
                    OverwriteOn0('|');
                    OverwriteOn0('/');
                    OverwriteOn0('-');
                    OverwriteOn0('\\');
                }
            });
        }

        static async Task Main(string[] args)
        {
            Debug.WriteLine($"Main:{System.Threading.Thread.CurrentThread.ManagedThreadId}", "SYNC");

            MocPrace();

            Vrtule(0, 0);

            await Vrtule(5, 0);

        }
    }
}
