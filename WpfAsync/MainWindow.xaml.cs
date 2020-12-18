﻿using System.Threading.Tasks;
using System.Windows;
using System.Threading;

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
        private static CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken ct = cts.Token;

        public event PropertyChangedEventHandler PropertyChanged;

        // Dvě vlastnosti, které jsou zapojené do data bindingu z XAML
        // jsou Counter a Vrtule (viz MainWindow.xaml)
        private string counter;
        public string Counter
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


        private void Button_Click_Prace(object sender, RoutedEventArgs e)
        {
            // Pošli cyklus na threadpool thread
            Task.Run(() =>
            {
                for (int i = 15; i > 0; i--)
                {
                    Counter = i.ToString();

                    // Jsme na threadpool threadu, takže nezablokujeme UI thread
                    // Vteřinu počká, než změní obsah Counter
                    Thread.Sleep(1000);
                }

                // Všichni, kdo mají token z cts zdroje dostanou informaci
                // o požadavku na zastavení. (ct.IsCancellationRequested bude true)
                cts.Cancel();

                Counter = "";
            });
        }

        private void Button_Click_Vrt(object sender, RoutedEventArgs e)
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
        }
    }
}