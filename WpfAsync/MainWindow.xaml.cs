using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;

// pro binding!!
using System.ComponentModel;

namespace WpfAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// INotifyPropertyChanged je interface, který obsahuje PropertyChanged!
    /// Binding je závislý na PropertyChanged!!!
    /// 
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // pokud chceme signalizovat ostatním vláknům požadavek na zastavení,
        // musíme vytvořit "token" - značku, která bude reagovat na změnu ve zdroji - "source"
        private static CancellationTokenSource cts;
        private CancellationToken ct;

        public event PropertyChangedEventHandler PropertyChanged;

        // Dvě vlastnosti, které jsou zapojené do data bindingu z XAML
        // jsou Counter a Vrtule (viz MainWindow.xaml)
        private int counter;
        public int Counter
        {
            get { return counter; }
            set
            {
                counter = value;
                // Dej vědět bindingu, jaké vlastnost se změnila, ať si aktualizuje UI
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Counter"));
            }
        }

        private string vrtule;
        public string Vrtule
        {
            get { return vrtule; }
            set
            {
                vrtule = value;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Vrtule"));
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // !!
            // Binding musí vědět, v jaké oblasti hledat vlastnosti Counter a Vrtule.
            // Jinak spadne s výjimkou
            // !!
            this.DataContext = this;
        }

        private async void Prace()
        {
            for (int i = 15; i >= 0; i--)
            {
                Counter = i;

                // Jsme na threadpool threadu, takže nezablokujeme UI thread
                // Vteřinu počká, než změní obsah Counter
                await Task.Delay(1000);
                //Thread.Sleep(1000);
            }

            // Všichni, kdo mají token z cts zdroje dostanou informaci
            // o požadavku na zastavení. (ct.IsCancellationRequested bude true)
            cts.Cancel();
        }
    

        private void Button_Click_Prace(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            ct = cts.Token;

            /**/
            // Pošli cyklus na threadpool thread
            Task.Run(async () =>
                {
                    for (int i = 15; i >= 0; i--)
                    {
                        Counter = i;

                        // Jsme na threadpool threadu, takže nezablokujeme UI thread
                        // Vteřinu počká, než změní obsah Counter
                        await Task.Delay(1000);
                    }

                    // Všichni, kdo mají token z cts zdroje dostanou informaci
                    // o požadavku na zastavení. (ct.IsCancellationRequested bude true)
                    cts.Cancel();
                });
            /**/

            /////////////// Lambda je "syntactic sugar" a hodně trendy
            // Dalo by se to napsat i takto, bez lambdy:
            // Task.Run(Prace);
            /////////////// Stačí jen zakomentovat verzi s lambdou a odkomentovat tuto část
        }

        // Když chceme, aby asynchronní metoda něco vracela, používáme generickou konstrukci
        // Task<návratový_typ> a musíme zajistit, aby spuštěná lambda vracela návratový_typ
        // protože je metoda async, musíme await Task:
        private async Task<int> TocSeVrtuleAsync()
        {
            int i = 0;
            await Task.Run( async () =>
            {
                while (ct.IsCancellationRequested == false)
                {
                    i++;
                    Vrtule = "-";

                    // Následující volání Sleep() nezablokují UI thread, protože běží na threadpool threadu
                    await Task.Delay(500);
                    Vrtule = "\\";
                    await Task.Delay(500);
                    Vrtule = "|";
                    await Task.Delay(500);
                    Vrtule = "/";
                    await Task.Delay(500);
                    Vrtule = "/";
                    await Task.Delay(500);
                }
                Vrtule = "";

            });
            return i;
        }

        private async Task<int> TocSeVrtuleAsync(int delay = 1500)
        {
            int i = 0;
            await Task.Run(async () =>
            {
                while (ct.IsCancellationRequested == false)
                {
                    i++;
                    Vrtule = "-";

                    // Následující volání Sleep() nezablokují UI thread, protože běží na threadpool threadu
                    await Task.Delay(delay);
                    Vrtule = "\\";
                    await Task.Delay(delay);
                    Vrtule = "|";
                    await Task.Delay(delay);
                    Vrtule = "/";
                    await Task.Delay(delay);
                    Vrtule = "/";
                    await Task.Delay(delay);
                }
                Vrtule = "";

            });
            return i;
        }

        private async void Button_Click_Vrt(object sender, RoutedEventArgs e)
        {

            // Zablokuje UI thread
            /*
            VrtText.Text = "-";
            Thread.Sleep(1000);
            VrtText.Text = "\\";
            Thread.Sleep(1000);
            VrtText.Text = "|";
            Thread.Sleep(1000);
            VrtText.Text = "/";
            Thread.Sleep(1000);
            VrtText.Text = "/";
            Thread.Sleep(1000);
            */


            // Vyhodí výjimku, protože operace na UI může dělat jen UI thread
            /*
            Task.Run(() =>
            {
                VrtText.Text = "-";
                Thread.Sleep(1000);
                VrtText.Text = "\\";
                Thread.Sleep(1000);
                VrtText.Text = "|";
                Thread.Sleep(1000);
                VrtText.Text = "/";
                Thread.Sleep(1000);
                VrtText.Text = "/";
                Thread.Sleep(1000);
            });
            */

            /* Tohle bude fungovat
            Task.Run(() =>
            {
                // Dokud na zdroji, který obsahuje ct (což je cts) nebude zavolaná metoda Cancel()
                while (ct.IsCancellationRequested == false)
                {
                    Vrtule = "-";

                    // Následující volání Sleep() nezablokují UI thread, protože běží na threadpool threadu
                    Thread.Sleep(500);
                    Vrtule = "\\";
                    Thread.Sleep(500);
                    Vrtule = "|";
                    Thread.Sleep(500);
                    Vrtule = "/";
                    Thread.Sleep(500);
                    Vrtule = "/";
                    Thread.Sleep(500);
                }
                Vrtule = "";
            });
            */

            // kdybychom nepoužili await, program by zde skončil v nekonečném cyklu Vrtule
            // int pocetOtacek = await TocSeVrtuleAsync();

            // override TocSeVrtule s možností nastavit rychlost otáčení
            int pocetOtacek = await TocSeVrtuleAsync(800);
            Vrtule = pocetOtacek.ToString();
        }
    }
}
