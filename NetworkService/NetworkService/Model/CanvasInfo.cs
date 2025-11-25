using NetworkService.Helpers;
using NetworkService.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NetworkService.Model
{
    public class CanvasInfo : BindableBase
    {
        private Brush statusColor = Brushes.Transparent;
        public Brush StatusColor
        {
            get => statusColor;
            set
            {
                statusColor = value;
                OnPropertyChanged("StatusColor");
            }
        }

        Entitie entitet;
        bool taken;
        int x, y, id;
        ObservableCollection<int> lines;
        public CanvasInfo(int ind)
        {
            Taken = false;
            Entitet = new Entitie();
            Id = ind;
            X = 10 + (ind % 4 + 1) * 100 + (ind % 4) * 160;
            Y = 85 + (ind / 4) * 200;
            lines = new ObservableCollection<int>();
        }

        public CanvasInfo(Entitie entitet, bool taken, int ind)
        {
            this.Entitet = entitet;
            this.Taken = taken;
            Id = ind;
            X = 10 + (ind % 4 + 1) * 100 + (ind % 4) * 160;
            Y = 85 + (ind / 4) * 200;
            lines = new ObservableCollection<int>();
        }

        public Brush Background
        {
            get
            {
                if (Entitet.Type != null)
                {
                    BitmapImage slika = new BitmapImage();
                    slika.BeginInit();
                    slika.UriSource = new Uri(Environment.CurrentDirectory + "../../../" + Entitet.Type.Img_src);
                    slika.EndInit();
                    return new ImageBrush(slika);
                }
                else
                    return Brushes.GhostWhite;
            }
        }
        public string Text { get => Entitet != null ? $"ID: {Entitet.Id}\nNaziv: {Entitet.Name}\nTip: {Entitet.Type?.Name}\nVrednost: {Entitet.Valued}" : ""; }
        public Brush Foreground { get => Uslov() ? Brushes.Blue : Brushes.Red; }

        // Metoda provjerava da li je vrijednost entiteta validna prema T1 specifikaciji.
        public bool Uslov()
        {
            if (Entitet != null)
            {
                // Prema T1 specifikaciji, validna vrijednost je između 5 i 16.
                return (Entitet.Valued >= 5 && Entitet.Valued <= 16);
            }

            return true; // U slučaju da entitet nije postavljen, smatramo da je uslov ispunjen (nema razloga za crveni prikaz).
        }

        public bool Taken
        {
            get => taken;
            set
            {
                if (taken != value)
                {
                    taken = value;
                    OnPropertyChanged("Taken");
                }
            }
        }
        public Entitie Entitet
        {
            get => entitet;
            set
            {
                entitet = value;
                OnPropertyChanged("Entitet");
                OnPropertyChanged("Foreground");
                OnPropertyChanged("Text");
            }
        }

        public int X
        {
            get => x;
            set
            {
                x = value;
                OnPropertyChanged("X");
            }
        }
        public int Y
        {
            get => y;
            set
            {
                y = value;
                OnPropertyChanged("Y");
            }
        }

        public ObservableCollection<int> Lines
        {
            get => lines;
            set { Lines = value; OnPropertyChanged("Lines"); }
        }

        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }
    }
}