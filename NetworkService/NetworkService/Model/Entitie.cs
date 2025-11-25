using NetworkService.Helpers;
using NetworkService.ViewModel;
using NetworkService.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetworkService.Model
{
    public class Entitie : ValidationBase
    {
        int id;
        string name;
        double valued;
        Type type;

        public Entitie()
        {

        }

       
        public Entitie(int id, string name, double valued, Type type)
        {
            this.Id = id;
            this.Name = name;
            this.Valued = valued;
            this.Type = type;
        }

        public Entitie(Entitie en)
        {
            this.Id = en.Id;
            this.Name = en.Name;
            this.Valued = en.Valued;
            this.Type = en.Type;
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
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public double Valued
        {
            get => valued;
            set
            {
                if (valued != value)
                {
                    valued = value;
                    OnPropertyChanged("Valued");

                    // Provera validnosti i promena boje na Canvas-u
                    if (NetworkDisplayViewModel.Canvases != null)
                    {
                        foreach (var canvas in NetworkDisplayViewModel.Canvases)
                        {
                            if (canvas.Entitet != null && canvas.Entitet.Id == this.Id)
                            {
                                // Ako je van opsega, crvena; inače transparent
                                canvas.StatusColor = (value < 5 || value > 16) ? Brushes.Red : Brushes.Transparent;
                            }
                        }
                    }

                    MeasurementGraphViewModel.OnIncomingValue(value, this.Id);
                }
            }
        }
        public Type Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }

        protected override void ValidateSelf()
        {
            if (this.Id <= 0)
            {
                this.ValidationErrors["Id"] = "ID mora biti veći od 0 i mora biti broj.";
            }
            else
            {
                // Napomena: petlja proverava postojeci staticki popis entiteta.
                // Pretpostavka je da je to ViewModel.NetworkEntitiesViewModel.Entiteti.
                foreach (Entitie entitet in ViewModel.NetworkEntitiesViewModel.Entiteti)
                {
                    if (entitet.Id == this.Id && !object.ReferenceEquals(entitet, this))
                        this.ValidationErrors["Id"] = "Ne možete imati dva ista ID-a.";
                }
            }

            if (string.IsNullOrWhiteSpace(this.Name))
            {
                this.ValidationErrors["Name"] = "Naziv je obavezan.";
            }

            if (type == null)
            {
                this.ValidationErrors["Type"] = "Tip je obavezan.";
            }
        }

        public override string ToString()
        {
            return $"ID: {Id}, Naziv: {Name}, Tip: {Type.Name}";
        }
    }
}