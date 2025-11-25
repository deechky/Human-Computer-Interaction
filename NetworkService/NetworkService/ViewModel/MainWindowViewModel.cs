using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkService.ViewModel
{

    public class MainWindowViewModel : BindableBase
    {
        //Prozori i manipulacija
        public MyICommand<string> NavCommand { get; private set; }
        public MyICommand ChangeCommand { get; set; }
		public MyICommand<Window> CloseWindowCommand { get; private set; }

		NetworkDisplayViewModel networkDisplayViewModel = new NetworkDisplayViewModel();
        NetworkEntitiesViewModel networkEntitiesViewModel = new NetworkEntitiesViewModel();
        MeasurementGraphViewModel measurementGraphViewModel = new MeasurementGraphViewModel();


        BindableBase currentViewModel;

        public BindableBase CurrentViewModel
        {
            get => currentViewModel;
            set
            {
                SetProperty(ref currentViewModel, value);
                
            }
        }

        public static bool UseToolTips { get; set; } = true;
        public MainWindowViewModel()
        {
            createListener(); //Povezivanje sa serverskom aplikacijom
            NavCommand = new MyICommand<string>(OnNav);
            ChangeCommand = new MyICommand(Change);
			CloseWindowCommand = new MyICommand<Window>(CloseWindow);
			CurrentViewModel = networkEntitiesViewModel;
        }
        private void Change()
        {
            if (CurrentViewModel == networkDisplayViewModel)
                CurrentViewModel = measurementGraphViewModel;
            else if (CurrentViewModel == measurementGraphViewModel)
                CurrentViewModel = networkEntitiesViewModel;
            else if (CurrentViewModel == networkEntitiesViewModel)
                CurrentViewModel = networkDisplayViewModel;
        }

        private void OnNav(string dest)
        {
            switch (dest)
            {
                case "NetEnt":
                    CurrentViewModel = networkEntitiesViewModel;
                    break;
                case "NetDis":
                    CurrentViewModel = networkDisplayViewModel;
                    break;
                case "MesGraph":
                    CurrentViewModel = measurementGraphViewModel;
                    break;             
            }
        }

        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25565); //kreira se objekat TCP koji slusa na odredjenom portu
            tcp.Start();     //pokrece se proces slusanja

            var listeningThread = new Thread(() =>   //glavna petlja slusanja
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient(); //blokira dok ne stigne klijent
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        incomming = Encoding.ASCII.GetString(bytes, 0, i).Trim();

                        // Ako simulator pita za broj objekata
                        if (incomming.Equals("Need object count"))
                        {
                            Byte[] data = Encoding.ASCII.GetBytes(NetworkEntitiesViewModel.Entiteti.Count.ToString());
                            stream.Write(data, 0, data.Length);
                        }
                        else
                        {
                            // Primer poruke: ID:1,Naziv:Kablovski senzor,P:10.23
                            Console.WriteLine("Primljeno: " + incomming);

                            if (NetworkEntitiesViewModel.Entiteti.Count > 0)
                            {
                                var parts = incomming.Split(',');
                                if (parts.Length == 3)
                                {
                                    var idPart = parts[0].Split(':');
                                    var nazivPart = parts[1].Split(':'); // može da se koristi kasnije
                                    var valuePart = parts[2].Split(':');

                                    int parsedId;
                                    double parsedValue;

                                    if (idPart.Length == 2 && valuePart.Length == 2 &&
                                        int.TryParse(idPart[1], out parsedId) &&
                                        double.TryParse(valuePart[1], out parsedValue))
                                    {
                                        DateTime dt = DateTime.Now;
                                        using (StreamWriter sw = File.AppendText("Log.txt"))
                                        {
                                            sw.WriteLine($"{dt}: ID={parsedId}, Vrednost={parsedValue}");
                                        }

                                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            if (parsedId >= 0 && parsedId < NetworkEntitiesViewModel.Entiteti.Count)
                                            {
                                                var entity = NetworkEntitiesViewModel.Entiteti[parsedId];
                                                entity.Valued = parsedValue;
                                                NetworkDisplayViewModel.UpdateList(entity);
                                                MeasurementGraphViewModel.OnIncomingValue(parsedValue, parsedId);
                                            }
                                            else
                                            {
                                                Console.WriteLine($"Primljena vrednost za nepoznat entitet index: {parsedId}");
                                            }
                                        }));
                                    }
                                }
                            }
                        }
                    }, null);
                }
            });

            listeningThread.IsBackground = true;
            listeningThread.Start();
        }


        private void CloseWindow(Window MainWindow)
		{
			MainWindow.Close();
		}
	}
}
