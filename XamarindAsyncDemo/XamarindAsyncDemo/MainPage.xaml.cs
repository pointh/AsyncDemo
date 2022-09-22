using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms;
using System.Diagnostics;
using Xamarin.Essentials;

namespace XamarindAsyncDemo
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        // Tohle nefunguje, protože si veškerý výkon vezme for cyklus a
        // nenechá UI vlákno překreslit formulář.
        // Takže přestože je obsah polí nastavený správně, jak vidíme v Debug,
        // uživatel jej neuvidí.
        private async void Start_ClickedSync(object sender, EventArgs e)
        {
            int i = 1, j = 1;

            for (; ; )
            {
                await Task.Delay(1000);
                Fast.Text = $"{i++}";

                if (i % 5 == 0)
                {
                    Slow.Text = $"{j++}";
                    Debug.WriteLine($"Slow.Text změna : {Slow.Text}");
                }
                Debug.WriteLine($"Fast.Text={Fast.Text}, Slow.Text={Slow.Text}");
            }
        }

        // Zkusíme poslat vykonávání cyklu for na thread pool.
        // Skončí chybou, protože do UI vlákna nemohou zasahovat
        // ostatní vlákna
        // Abychom mohli použít await, musíme mít async metodu.
        private async void Start_ClickedFail(object sender, EventArgs e)
        {
            int i = 1, j = 1;

            await Task.Run(async () =>
            {
                for (; ; )
                {
                    await Task.Delay(1000);
                    Fast.Text = $"{i++}";

                    if (i % 5 == 0)
                    {
                        Debug.WriteLine($"Slow start j={j}");
                        Slow.Text = $"{j++}";
                    }
                    Debug.WriteLine($"i={i}, j={j}");
                }
            });
        }

        // Necháme for pracovat v thread poolu a pomocí metody z 
        // Xamarin Essentials MainThread.BeginInvokeOnMainThread
        // necháme "překreslovací" část kódu vykonat na UI (main) vlákně.
        private async void Start_ClickedAsync(object sender, EventArgs e)
        {
            int i = 1, j = 1;

            await Task.Run(async () =>
            {
                for (; ; )
                {
                    await Task.Delay(1000);
                    // Změnu můžeme provést jenom na UI threadu
                    MainThread.BeginInvokeOnMainThread(() => { Fast.Text = $"{i++}"; });

                    if (i % 5 == 0)
                    {
                        Debug.WriteLine($"Slow start j={j}");
                        // Změnu můžeme provést jenom na UI threadu
                        MainThread.BeginInvokeOnMainThread(() => { Slow.Text = $"{j++}"; });
                    }
                    Debug.WriteLine($"i={i}, j={j}");
                }
            });
        }

        static CancellationTokenSource cancelSource = new CancellationTokenSource();
        CancellationToken ct = cancelSource.Token;

        private async void Start_ClickedAsyncStoppable(object sender, EventArgs e)
        {
            // Jako switch, pokud je možnost ukončit,
            // nastaví cancelSource svůj token na ukončení voláním cancelSource.Cancel();
            if (Start.Text == "Stop")
            {
                Start.Text = "Start";
                cancelSource.Cancel();
                return;
            }

            int i = 1, j = 0;
            Start.Text = "Stop";
            Slow.Text = $"{j}"; // hlavně kvůli restartu counterů, aby se nečekalo na
                                // další aktualizaci proměnné j

            await Task.Run(async () =>
            {
                for (; ; )
                {
                    MainThread.BeginInvokeOnMainThread(() => { Fast.Text = $"{i++}"; });

                    if (i % 5 == 0)
                    {
                        Debug.WriteLine($"Slow start j={j}");
                        MainThread.BeginInvokeOnMainThread(() => { Slow.Text = $"{++j}"; });
                    }
                    Debug.WriteLine($"i={i}, j={j}");
                    await Task.Delay(1000);

                    // pokud cancelSource zavolal Cancel(), je v odpovídajícím tokenu
                    // ct nastaveno IsCancellationRequested na true
                    if (ct.IsCancellationRequested)
                    {
                        // necháme GC zrušit starý cancelSource a vytvoříme nový
                        // s připojeným cancellation tokenem
                        // a ukončíme nekonečný cyklus
                        cancelSource = new CancellationTokenSource();
                        ct = cancelSource.Token;
                        break;
                    }
                }
            });
        }
    }
}
