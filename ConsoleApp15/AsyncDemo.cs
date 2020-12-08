using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Diagnostics;

namespace AsyncDemoNS
{
    class AsyncDemo
    {
        public static void MocPrace()
        {
            for (int i = 0; i < 100; i++)
            {
                System.Threading.Thread.Sleep(1000);
                Console.SetCursorPosition(10, 10);
                Console.Write(i);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("1 problém");
            Console.SetCursorPosition(0, 0);

            MocPrace();

            Console.CursorVisible = false;
            while (true)
            {
                Console.Write('|');
                Console.SetCursorPosition(0, 0);
                System.Threading.Thread.Sleep(500);

                Console.Write('/');
                Console.SetCursorPosition(0, 0);
                System.Threading.Thread.Sleep(500);

                Console.Write('-');
                Console.SetCursorPosition(0, 0);
                System.Threading.Thread.Sleep(500);

                Console.Write('\\');
                Console.SetCursorPosition(0, 0);
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
