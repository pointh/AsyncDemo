using System.Diagnostics;

namespace ReturnAsyncValues
{
    internal class Program
    {
        static async Task<int> GetNumberAsync()
        {
            Console.WriteLine("Zahajuji GetNumberAsync...");
            await Task.Delay(1500); // Simulace asynchronní operace
            Console.WriteLine("Dokončuji GetNumberAsync.");
            return 42;
        }

        static async Task<List<int>> GetListAsync()
        {
            Console.WriteLine("Zahajuji GetListAsync...");
            await Task.Delay(800); // Simulace asynchronní operace
            Console.WriteLine("Dokončuji GetListAsync.");
            return new List<int> { 1, 2, 3, 4, 5 };
        }   

        static async Task<Dictionary<string, int>> GetDictionaryAsync()
        {
            Console.WriteLine("Zahajuji GetDictionaryAsync...");
            await Task.Delay(300); // Simulace asynchronní operace
            Console.WriteLine("Dokončuji GetDictionaryAsync.");
            return new Dictionary<string, int>
            {
                { "One", 1 },
                { "Two", 2 },
                { "Three", 3 }
            };
        }

        static async Task Main(string[] args)
        {
            // Definice úloh
            var task1 = GetNumberAsync();           // Task<int>
            var task2 = GetListAsync();             // Task<List<int>>
            var task3 = GetDictionaryAsync();       // Task<Dictionary<string, int>>

            Stopwatch sw = Stopwatch.StartNew();

            await Task.WhenAll(task1, task2, task3);

            sw.Stop();

            int number = task1.Result;
            List<int> list = task2.Result;
            Dictionary<string, int> dict = task3.Result;

            // Přímý přístup k hodnotám
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine($"Číslo: {number}");
            Console.WriteLine($"Prvky v listu: {list.Count}");
            Console.WriteLine($"Klíčů v dictionary: {dict.Count}");
            Console.WriteLine($"Celkový čas: {sw.ElapsedMilliseconds} ms");
        }
    }
}
