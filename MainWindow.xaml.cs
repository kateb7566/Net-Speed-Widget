using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;

namespace Net_Speed_Widget
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //synchronized string kira = "hello world !";
        private readonly BackgroundWorker worker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            Speedy.Text = "0";
            //new Thread(updateSpeed).Start();
            worker.DoWork += updateSpeed;
            worker.RunWorkerCompleted += fint;
            worker.RunWorkerAsync();
        }

        private void updateSpeed(object sender, DoWorkEventArgs e)
        {

            Thread.Sleep(TimeSpan.FromSeconds(5));
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                (ThreadStart)delegate ()
                    {
                        if (SpeedStats.isConnected())
                        {
                            rConnect.Fill = Brushes.LightGreen;
                            Speedy.Text = SpeedStats.CalcSpeed();
                        }
                        else
                        {
                            rConnect.Fill = Brushes.Red;
                            Speedy.Text = "0";
                        }
                    });
        }

        private void fint(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null) MessageBox.Show("Error ! : Calculate Error !");
        }

        public string netSpeed
        {
            get { return (string)GetValue(netSpeedProperty); }
            set { SetValue(netSpeedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for netSpeed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty netSpeedProperty =
            DependencyProperty.Register("netSpeed",
                typeof(string), 
                typeof(MainWindow), 
                new PropertyMetadata(null));



        private void Tester_Click(object sender, RoutedEventArgs e)
        {

            worker.DoWork += updateSpeed;
            worker.RunWorkerCompleted += fint;
            worker.RunWorkerAsync();

            // ping google.com
            //if (SpeedStats.isConnected())
            //{
            //    rConnect.Fill = Brushes.LightGreen;
            //    Speedy.Text = SpeedStats.CalcSpeed();
            //}
            //else
            //{
            //    rConnect.Fill = Brushes.Red;
            //    Speedy.Text = "0";
            //}
        }

        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            if (DarkMode.IsChecked == true)
            {
                bandrolle.Background = Brushes.Gray;
                Theme.Background = Brushes.Black;
                Speedy.Foreground = Brushes.White;
                megab.Foreground = Brushes.White;
                rights.Foreground = Brushes.White;
                Title.Foreground = Brushes.White;
                DarkMode.Background = Brushes.White;
            }
            else
            {
                bandrolle.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#313131"));
                Theme.Background = Brushes.White;
                Speedy.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
                megab.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
                rights.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#757575"));
                Title.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#212121"));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }

    public class SpeedStats : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private string speed = "0";
        public string netSpeed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
                PropertyChanged(this, new PropertyChangedEventArgs("netSpeed"));
            }
        }

        // protect connection status
        public bool connect
        {
            get;
            set;
        }

        // ping a server and get a reply
        public static bool isConnected()
        {
            // throw an exception
            Ping ping = new Ping();
            try
            {
                PingReply reply = ping.Send("www.google.com", 5000);
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
            }
            catch
            {
                MessageBox.Show("There is no internet connection",
                    "Net Speed Widget",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            return false;
        }

        public static string CalcSpeed()
        {
            //string x = "NaN";
            double speed = 0;
            for (int i = 0; i < 10; i++)
            {

                Stopwatch watch = new Stopwatch(); //using system.diagnostics
                watch.Start();
                WebClient web = new WebClient();
                byte[] bytes = web.DownloadData("https://www.google.com");
                watch.Stop();
                double sec = watch.Elapsed.TotalSeconds;
                speed += bytes.Count() / sec;
                Thread.Sleep(1000);
            }

            // calculate debit per 10 secs
            return string.Format("{0:F2}", speed * 32 / 10485760);
        }

        private static ReaderWriterLockSlim reda = new ReaderWriterLockSlim();

        

        public static SpeedStats GetSpeed()
        {
            reda.EnterWriteLock();
            SpeedStats spd;
            try
            {
                spd = new SpeedStats()
                {
                    netSpeed = CalcSpeed()
                };
            }
            finally
            {
                reda.ExitWriteLock();
            }

            return spd;
        }
    }
}
