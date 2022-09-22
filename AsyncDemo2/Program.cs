using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace AsyncDemo2
{
    class Program
    {
        private static async Task<string> A()
        {
            Console.WriteLine("Začínám A");
            await Task.Delay(3000);
            return "AReturn";
        }

        private static async Task B()
        {
            Console.WriteLine("Začínám B");
            await Task.Delay(6000);
            Console.WriteLine("B");
        }

        private static async Task C()
        {
            // opravdu úlohy čekají celých 9 s do ukončení?
            Console.WriteLine($"Začínám C v {DateTime.Now:m:ss}");
            await Task.Delay(9000);
            Console.WriteLine($"C v {DateTime.Now:m:ss}");
        }

        private static async Task Z(int i)
        {
            Console.WriteLine($"Začínám Z{i}");
            await Task.Delay(10000);
            Console.WriteLine($"Z{i}");
        }

        static async Task Main(string[] args)
        {
            List<Task> tList = new List<Task>();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Task<string> t; // aspoň jedna metoda něco vrací...
            Task u, v;
            t = Task.Run(A);
            u = Task.Run(B);
            v = Task.Run(C);

            tList.AddRange(new Task[] { t, u, v });

            // Trochu zvýšíme zatížení threadpoolu. Asynchronně spustíme 100 + 3 úloh
            // "Funkcionální" přidání úloh do tList:
            // Enumerable.Range vrací sekvenci Enumerable,
            // Select z každého i vytvoří volání Task.Run(() => Z(i))
            tList.AddRange(Enumerable.Range(0, 100).Select(i => Task.Run(() => Z(i))));

            // Již v tomto okamžiku některé z úloh ze seznamu tList běží,
            // threadpool scheduler je startuje v pořadí, které nesouvisí
            // s pořadím ukládání tasků do tList
            Console.WriteLine($"Začátek v {sw.ElapsedMilliseconds}"); 
            // Thread.Sleep(10000);
            // Je vcelku jedno, jestli bude hlavní vlákno spát, všechno
            // pojede na threadpoolu       

            Console.WriteLine("Výsledek tasku t");
            Console.WriteLine(await t); // await neovlivní zpracování ostatních 102 úloh!
            // To se jen informace "t dokončeno" vždy vypíše až po vypsání návratové hodnoty t
            // (tedy volání A())
            Console.WriteLine($"{sw.ElapsedMilliseconds}, t dokončeno");
            
            // Pokud nebude program čekat pomocí WailAll, program dojede na konec
            // a úlohy spuštěné na threadpoolu se nedokončí (zaniknou s Main()).
            Console.WriteLine("Čekání na pole tasků");
            Task.WaitAll(tList.ToArray()); // WailAll pracuje pouze s polem

            // Konec bude poslední, co program vypíše
            Console.WriteLine($"{sw.ElapsedMilliseconds}, Konec");

            // Celkový čas zpracování je podstatně kratší, než je součet
            // časů všech spuštěných úloh!
        }
    }
}
