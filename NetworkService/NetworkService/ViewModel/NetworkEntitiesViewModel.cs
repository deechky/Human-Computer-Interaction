using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        public static ObservableCollection<Entitie> Entiteti { get; set; }  //Lista entiteta
        public static ObservableCollection<Entitie> temp { get; set; }      //Pomocna lista koja ce mi trebat za Pretragu
        public static ICollectionView PrikazEntiteta { get; set; }          //Kolekcija za prikaz entiteta  u tabeli
        public static List<Model.Type> Tipovi { get; set; }                 //Tip entiteta
        public MyICommand AddCommand { get; set; }                          //Komanda za dodavanje
        public MyICommand DeleteCommand { get; set; }                      //Komanda za brisanje
        public MyICommand SearchCommand { get; set; }                       // Komanda za P1 pretragu
        public MyICommand CancelSearchCommand { get; set; }
        public MyICommand HelpCommand { get; set; }                       //Help komanda
        public MyICommand ToggleToolTipsCommand { get; set; }
        public MyICommand UndoCommand { get; set; }

        // UNDO podrška za CG5
        private Entitie lastDeletedEntity = null;

        bool toolTipsBool;                                                    //bool promjenljiva za ToolTip

        // P1 pretraga properties
        private string searchText = "";
        private bool searchByName = true;
        private bool searchcan = false;

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                OnPropertyChanged("SearchText");
                SearchCommand.RaiseCanExecuteChanged();
            }
        }

        public bool SearchByName
        {
            get => searchByName;
            set
            {
                searchByName = value;
                OnPropertyChanged("SearchByName");
                SearchCommand.RaiseCanExecuteChanged();
            }
        }

        public bool SearchByType
        {
            get => !searchByName;
            set
            {
                searchByName = !value;
                OnPropertyChanged("SearchByName");
                OnPropertyChanged("SearchByType");
                SearchCommand.RaiseCanExecuteChanged();
            }
        }

        public bool ToolTipsBool                                              //Property za ToolTip
        {
            get => toolTipsBool;
            set
            {
                toolTipsBool = value;
                MainWindowViewModel.UseToolTips = value;
                OnPropertyChanged("ToolTipsBool");
            }
        }

        Entitie noviEntitet = new Entitie();                                  //Entitet za tabelu
        public Entitie NoviEntitet                                             //Property za Entitet
        {
            get => noviEntitet;
            set
            {
                noviEntitet = value;
                OnPropertyChanged("NoviEntitet");
            }
        }

        Entitie izabran;                                                        //Entitet za brisanje, onaj koji smo selektovali
        public Entitie Izabran                                                 //Property
        {
            get => izabran;
            set
            {
                izabran = value;
                DeleteCommand.RaiseCanExecuteChanged();
            }
        }


        public NetworkEntitiesViewModel()                                  //Inicijalizacija  listi i komandi
        {
            //potrebna inicijalizacija
            if (Entiteti == null)
            {
                Entiteti = new ObservableCollection<Entitie>();
            }
            PrikazEntiteta = CollectionViewSource.GetDefaultView(Entiteti);
            temp = new ObservableCollection<Entitie>();

            // Inicijalizacija tipova za T1 specifikaciju
            Tipovi = new List<Model.Type> { new Model.Type("Kablovski senzor"), new Model.Type("Digitalni manometar") };

            //komande
            AddCommand = new MyICommand(OnAdd);
            DeleteCommand = new MyICommand(OnDelete, CanDelete);
            SearchCommand = new MyICommand(OnSearch, CanSearch);
            CancelSearchCommand = new MyICommand(OnCancelSearch, CanCancelSearch);
            HelpCommand = new MyICommand(OnHelp);
            ToggleToolTipsCommand = new MyICommand(OnToggleToolTips);
            UndoCommand = new MyICommand(OnUndo, CanUndo);

            //Dodjelivanje odmah na pocektu potrebnih vrijednosti
            NoviEntitet.Id = 1;
            HelpText = saveHelp;
            ToolTipsBool = MainWindowViewModel.UseToolTips;

            // Dodavanje test entiteta za T1 - Pritisak u ventilima
            if (Entiteti.Count == 0)
            {
                AddTestEntity(1, "Ventil 1", "Kablovski senzor", 12.5);
                AddTestEntity(2, "Ventil 2", "Digitalni manometar", 8.9);
                AddTestEntity(3, "Ventil 3", "Kablovski senzor", 17.0); // Iznad opsega
            }
        }

        private void AddTestEntity(int id, string name, string typeName, double value)
        {
            var entity = new Entitie
            {
                Id = id,
                Name = name,
                Type = Tipovi.First(t => t.Name == typeName),
                Valued = value
            };
            Entiteti.Add(entity);
            temp.Add(entity);

            // Null check za NetworkDisplayViewModel.EntitetList
            if (NetworkDisplayViewModel.EntitetList != null)
            {
                NetworkDisplayViewModel.EntitetList.Add(entity);
            }

            NoviEntitet.Id = Math.Max(NoviEntitet.Id, id + 1);
        }

        private void OnToggleToolTips()
        {
            ToolTipsBool = !ToolTipsBool;
        }

        //Help
        string helpText;
        static string saveHelp = "";
        public string HelpText
        {
            get => helpText;
            set
            {
                helpText = value;
                saveHelp = value;
                OnPropertyChanged("HelpText");
                HelpCommand.RaiseCanExecuteChanged();
            }
        }
        //Komadna
        private void OnHelp()
        {
            if (HelpText == string.Empty)
            {
                HelpText =  "PREČICE:\n" +
                            "• CTRL+D → Dodavanje entiteta\n" +
                            "• CTRL+P → Pretraga (P1)\n" +
                            "• CTRL+Z → UNDO (poništi brisanje)\n" +
                            "• CTRL+T → Toggle ToolTips\n" +
                            "• CTRL+H → Help\n" +
                            "• CTRL+Tab → Navigacija između view-ova\n\n" +
                            "FUNKCIONALNOSTI:\n" +
                            "• PRETRAGA: Po nazivu ili tipu entiteta\n" +
                            "• Dodavanje/brisanje entiteta sa potvrdom\n" +
                            "• UNDO: Vraćanje poslednje obrisanog entiteta\n";
            }
            else
            {
                HelpText = string.Empty;
            }
        }

        //Dodavanje
        private void OnAdd()
        {
            NoviEntitet.Validate();   
            if (NoviEntitet.IsValid)  
            {
                if (ExistsID(NoviEntitet.Id))  
                {
                    NoviEntitet.ValidationErrors["Id"] = "ID postoji u listi.";
                    return;
                }

                var added = new Entitie(NoviEntitet);
                added.Valued = 0;
                Entiteti.Add(added);
                temp.Add(added);

                if (NetworkDisplayViewModel.EntitetList != null)
                {
                    NetworkDisplayViewModel.EntitetList.Add(added);
                }
                NoviEntitet.Id++;

                System.Windows.MessageBox.Show("Uspešno dodat entitet.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                MeasurementGraphViewModel.RefreshComboBoxData();
            }
        }
        //Brisanje
        private void OnDelete()
        {
            if (Izabran == null) return;
            var result = System.Windows.MessageBox.Show($"Da li ste sigurni da želite da obrišete entitet ID={Izabran.Id}?", "Potvrda", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            lastDeletedEntity = new Entitie(Izabran); // Čuvaj za UNDO

            NetworkDisplayViewModel.RemoveFromList(Izabran);   
            Entiteti.Remove(Izabran);                         
            temp.Remove(Izabran);                             

            System.Windows.MessageBox.Show("Entitet obrisan. CTRL+Z za Undo.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            UndoCommand.RaiseCanExecuteChanged();
            MeasurementGraphViewModel.RefreshComboBoxData(); 
            RestartSimulator();
        }
        private bool CanDelete()
        {
            return Izabran != null;
        }
        //Pretraga

        bool ExistsID(int id)     
        {
            foreach (Entitie e in Entiteti)
            {
                if (e.Id == id)
                {
                    return true;
                }
            }
            return false;
        }

        private void RestartSimulator()
        {
            try
            {
                foreach (var p in Process.GetProcessesByName("MeteringSimulator"))
                {
                    try { p.Kill(); } catch { }
                }
                string simPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\..\\MeteringSimulator - T1\\MeteringSimulator\\bin\\Debug\\MeteringSimulator.exe"));
                if (System.IO.File.Exists(simPath))
                {
                    var psi = new ProcessStartInfo(simPath)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    Process.Start(psi);
                }
            }
            catch { }
        }

        // P1 Pretraga metode
        private bool CanSearch()
        {
            return !string.IsNullOrWhiteSpace(SearchText);
        }

        private void OnSearch()
        {
            if (temp.Count == 0)
            {
                foreach (Entitie e in Entiteti)
                {
                    temp.Add(e);
                }
            }

            List<Entitie> searchResults = new List<Entitie>();
            string searchTerm = SearchText.ToLower().Trim();

            List<Entitie> sourceData = temp.Count > 0 ? temp.ToList() : Entiteti.ToList();

            foreach (Entitie entity in sourceData)
            {
                bool matches = false;

                if (SearchByName && entity.Name != null)
                {
                    matches = entity.Name.ToLower().Contains(searchTerm);
                }

                if (!matches && SearchByType && entity.Type != null)
                {
                    matches = entity.Type.Name.ToLower().Contains(searchTerm);
                }

                if (matches)
                {
                    searchResults.Add(entity);
                }
            }

            Entiteti.Clear();
            foreach (Entitie e in searchResults)
            {
                Entiteti.Add(e);
            }

            searchcan = true;
            CancelSearchCommand.RaiseCanExecuteChanged();
        }

        private bool CanCancelSearch()
        {
            return searchcan;
        }

        private void OnCancelSearch()
        {
            Entiteti.Clear();
            foreach (Entitie e in temp)
            {
                Entiteti.Add(e);
            }
            temp.Clear();
            searchcan = false;
            SearchText = "";
            CancelSearchCommand.RaiseCanExecuteChanged();
        }

        // UNDO funkcionalnost 
        private bool CanUndo()
        {
            return lastDeletedEntity != null;
        }

        private void OnUndo()
        {
            if (lastDeletedEntity != null)
            {
                Entiteti.Add(lastDeletedEntity);
                temp.Add(lastDeletedEntity);

                if (NetworkDisplayViewModel.EntitetList != null)
                {
                    NetworkDisplayViewModel.EntitetList.Add(lastDeletedEntity);
                }

                System.Windows.MessageBox.Show($"Vraćen entitet: {lastDeletedEntity.Name}", "Undo", MessageBoxButton.OK, MessageBoxImage.Information);
                lastDeletedEntity = null;
                UndoCommand.RaiseCanExecuteChanged();
                MeasurementGraphViewModel.RefreshComboBoxData();
                RestartSimulator();
            }
        }
    }
}
