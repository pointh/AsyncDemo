using System;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace AsyncDemoNS
{
    class AsyncDemo
    {
        // Pokud chceme mít možnost ukončit procesy, které jedou a vláknech v threadpoolu,
        // musíme pracovat s objektem CacellationToken. Ten funguje jako sdílený semafor pro 
        // všechna jedoucí vlákna. Je řízený pomocí CancellationTokenSource.
        // V našem případě se voláním 'source.Cancel()' nastaví stopka na ct a my se podle toho,
        // zda ct.IsCancellationRequested == true/false můžeme
        // rozhodnout, zda má program dál pokračovat
        private static readonly CancellationTokenSource source = new CancellationTokenSource();
        private static readonly CancellationToken ct = source.Token;

        // U vícevláknových programů je třeba zabránit tomu, aby se jednotlivá vlákna "přetahovala"
        // o sdílené zdroje. V tomto případě je sdíleným zdrojem Console.
        // Později v kódu uzamkneme operace na Console pro vlákno, dokud nedokončí svou práci a potom 
        // objekt Console otevřeme ostatním vláknům.
        private static readonly object lockObject = new object();


        public async static Task<int> MocPrace()
        {
            int i = 0;
            // Task.Run nespouští asynchronní metodu, ale pouze ji předá Scheduleru na threadpoolu

            // async () => {...} je popis lambda funkce - bezejmenné funkce bez argumentů, kterou definujeme uvnitř
            // závorek { a }
            await Task.Run(async () =>
            {
                for (i = 10; i > -1; i--)
                { 
                    // Pokud je semafor nastavený na "Končíme"
                    if (ct.IsCancellationRequested)
                    {
                        // Okamžitě ukonči program
                        Environment.Exit(200);
                    }
                    
                    await Task.Delay(500);
                    
                    // Zablokování Console pro aktuální vlákno,
                    // ostatní vlákna musejí čekat na dokončení bloku uvnitř lock(lockObject){...}
                    lock (lockObject)
                    {
                        Console.SetCursorPosition(10, 10);
                        Console.Write(i);

                        // Pokud uživatel zadal cokoliv z klávesnice,
                        // ukonči cyklus, ukonči všechna vlákna napojená na source
                        // a vrať aktuální hodnotu i
                        if (Console.KeyAvailable)
                        {
                            break;
                        }
                    }
                }
                // když skončíš cyklus, nastav semafor "Končíme" pro všechny, kto mají token ze source,
                // (tedy odkazují se na proměnnou ct typu CancellationToken)
                source.Cancel();
            });
            return i;
        }

        public async static Task Vrtule(int x, int y)
        {
            // NOVÉ!!!
            // toto je interní metoda, platná pouze v rozsahu metody Vrtule
            async Task OverwriteVrtule(char c, int x, int y)
            {
                await Task.Delay(500);

                lock (lockObject) {
                    Console.CursorVisible = false;
                    Console.SetCursorPosition(x, y);
                    Console.Write(c);
                }
            }

            await Task.Run(async () =>
            {
                while (true)
                {
                    // Pokud je na "semaforu" ct nastaveno "Končíme",
                    // ukonči provádění metody
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    await OverwriteVrtule('|', x, y);
                    await OverwriteVrtule('/', x, y);
                    await OverwriteVrtule ('-', x, y);
                    await OverwriteVrtule ('\\', x, y);
                }
            });
        }

        static void Main()
        {
            Console.CursorVisible = false;

            // Jen u této metody nás zajímá výsledek, proto si nadefinujeme proměnnou
            // Task<int>, kde <int> znamená, že po ukončení metody MocPrace v 
            // asynchronním režimu můžeme pracovat s celočíselnou navrácenou hodnotou
            Task<int> taskMocPrace = Task.Run(() => MocPrace());

            Task[] tasks =
            {
                taskMocPrace, // připravili jsme si v minulém kroku
                // spustíme 6x metodu Vrtule a necháme na Scheduleru threadpoolu,
                // aby si rozhodl, na jakém vláknu které volání pojede
                Task.Run(()=>Vrtule(14, 14)),
                Task.Run(()=>Vrtule(14, 15)),
                Task.Run(()=>Vrtule(14, 16)),
                Task.Run(()=>Vrtule(15, 14)),
                Task.Run(()=>Vrtule(15, 15)),
                Task.Run(()=>Vrtule(15, 16)),
            };

            try
            {
                // Počkáme si na dokončení všech úloh ve vláknech
                // Při násilném přerušení můžeme ukončit všechny rozjeté úlohy tím,
                // že do WailAll předáme i CancellationToken
                // Pokud k přerušení dojde, systém vyvolá výjimku OperationCancelledException,
                // v opačném případě program zajde přirozenou cestou a systém ukončí všechny úlohy na threadpoolu
                Task.WaitAll(tasks, ct);
            }
            catch (OperationCanceledException)
            {
                lock (lockObject)
                {
                    Console.SetCursorPosition(0, 20);
                    Console.WriteLine($"Operace přerušena na hodnotě {taskMocPrace.Result}");
                    Environment.Exit(taskMocPrace.Result);
                }
            }
        }
    }
}
