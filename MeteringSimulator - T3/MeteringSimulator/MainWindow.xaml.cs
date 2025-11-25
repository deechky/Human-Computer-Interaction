using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

namespace MeteringSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Pritisak meren u MP
        private static double value = -1;
        // ID objekta
        private static int objectNum = 0;
        // Tip merača (naziv)
        private static string gaugeType = "";
        // Ukupan broj objekata
        private int numObjects = -1;
        // Random generator za vrednosti i vreme
        private Random r = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Proveri broj objekata pod monitoringom
            askForCount();
            // Pocni prijavljivanje novih vrednosti za objekte
            startReporting();
        }

        private void askForCount()
        {
            try
            {
                // Pita koliko aplikacija ima objekata
                // Request
                Int32 port = 25565;
                using (TcpClient client = new TcpClient("localhost", port))
                {
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes("Need object count");
                    using (NetworkStream stream = client.GetStream())
                    {
                        stream.Write(data, 0, data.Length);

                        // Obrada odgovora
                        // Response
                        Byte[] responseData = new Byte[1024];
                        string response = "";
                        Int32 bytess = stream.Read(responseData, 0, responseData.Length);
                        response = System.Text.Encoding.ASCII.GetString(responseData, 0, bytess);

                        // Parsiranje odgovora u int vrednost
                        numObjects = Int32.Parse(response);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        private void startReporting()
        {
            // Na radnom vreme posalji izmenu vrednosti nekog random objekta i nastavi da to radis u rekurziji
            int waitTime = r.Next(1000, 5000);
            Task.Delay(waitTime).ContinueWith(_ =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    // Slanje izmene stanja nekog objekta
                    sendReport();
                    // Upis u text box, radi lakse provere
                    textBox.Text = $"ID: {objectNum}, Naziv: {gaugeType}, Vrednost: {value:F2} MP\n" + textBox.Text;
                    // Pocni proces ispocetka
                    startReporting();
                });
            });
        }

        private void sendReport()
        {
            try
            {
                // Slanje nove vrednosti objekta
                // Request
                Int32 port = 25565;
                using (TcpClient client = new TcpClient("localhost", port))
                {
                    // Izbor nasumičnog ID-a objekta
                    int rInt = r.Next(0, numObjects);
                    objectNum = rInt;

                    // Određivanje tipa merača (naziva) na osnovu ID-a
                    gaugeType = (rInt % 2 == 0) ? "Kablovski senzor" : "Digitalni manometar";

                    // Generisanje validne ili nevalidne vrednosti pritiska
                    // 10% šanse da se generiše nevalidna vrednost
                    if (r.Next(0, 10) == 0)
                    {
                        // Generiši nevalidnu vrednost (manje od 5 MP ili veće od 16 MP)
                        if (r.Next(0, 2) == 0)
                        {
                            // Nevalidna vrednost < 5 MP
                            value = r.NextDouble() * 5;
                        }
                        else
                        {
                            // Nevalidna vrednost > 16 MP
                            value = 16 + r.NextDouble() * 5;
                        }
                    }
                    else
                    {
                        // Generiši validnu vrednost (između 5 i 16 MP)
                        value = r.NextDouble() * (16 - 5) + 5;
                    }

                    // Formatiranje poruke za slanje
                    Byte[] data = System.Text.Encoding.ASCII.GetBytes($"ID:{objectNum},Naziv:{gaugeType},P:{value:F2}");
                    using (NetworkStream stream = client.GetStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        public void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }
    }
}