#define WHENALL
//#define WHENANY
//#define NOAWAIT
//#define JUSTAWAIT
namespace AsyncConsole1
{
    internal class Program
    {
        private static readonly object consoleLock = new object(); 
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Main: Thread {Thread.CurrentThread.Name} " +
                $"Id={Thread.CurrentThread.ManagedThreadId}");

#if NOAWAIT
            CountLeftAsync();
            CountRightAsync();
#endif

#if JUSTAWAIT
            await CountLeftAsync();
            await CountRightAsync();
#endif

            Task t = Task.Run(CountLeftAsync);
            Task v = Task.Run(CountRightAsync);
            Task[] tasks = new Task[] { t, v };
            // Task.Run je název zavádějící, 
            // ve skutečnosti se metody nespouštějí,
            // ale zařadí se do fronty pro zpracování.
            // Task.Run vrací "pointer" na úlohu,
            // a můžeme jím ovlivňovat její běh.

#if WHENALL
            await Task.WhenAll(tasks);
            // nebo rovnou Task.WhenAll(t, v);
            // Task.WhenAll vrací úlohu, která se 
            // ukončí, až budou všechny úlohy zpracované
#endif

#if WHENANY
            await Task.WhenAny(tasks);
#endif
            // Konec Main() ukončí u všechy 
            // spuštěné úlohy, pokud nejsou ukončené
            Console.WriteLine("Konec Main()");
        }

        static async Task CountLeftAsync()
        {
            for (int i = 0; i < 5; i++)
            {
                lock (consoleLock)
                {
                    Console.SetCursorPosition(0, 2);
                    ReportThread(i);
                }
                await Task.Delay(1000);
            }
        }

        // Běží rychleji!
        static async Task CountRightAsync()
        {
            for (int i = 5; i > 0; i--)
            {
                lock (consoleLock)
                {
                    Console.SetCursorPosition(60, 23);
                    ReportThread(i);
                }
                await Task.Delay(600);
            }
        }

        static void ReportThread(int i)
        {
            ThreadPool.GetMaxThreads(out int workerThreads, out int _);
            Console.WriteLine($"{i} : Thread {Thread.CurrentThread.Name} " +
                $"Id={Thread.CurrentThread.ManagedThreadId} z {workerThreads}");
        }
    }
}