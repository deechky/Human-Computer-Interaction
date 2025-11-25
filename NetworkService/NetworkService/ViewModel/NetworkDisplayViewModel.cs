using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NetworkService.Views;
using System.Windows.Input;
using NetworkService.Helpers;

namespace NetworkService.ViewModel
{
    public class NetworkDisplayViewModel : BindableBase
    {
        public static void RemoveFromList(Entitie e)      //Izbrisan na prvom prozoru, izbrisi i ovdje
        {
            foreach (Entitie entitet in EntitetList)
                if (entitet.Id == e.Id)
                {
                    EntitetList.Remove(entitet);
                    return;
                }

            for (int i = 0; i < 12; i++)                  //Izbrisi i liniju za taj entitet
                if (Canvases[i].Taken && Canvases[i].Entitet != null && Canvases[i].Entitet.Id == e.Id)
                {
                    foreach (int id in Canvases[i].Lines)
                        RemoveLine(id);
                    Canvases[i] = new CanvasInfo(i);
                    return;
                }
        }

        public static void UpdateList(Entitie e)           //Azuriraj listu
        {
            for (int i = 0; i < EntitetList.Count; i++)
                if (EntitetList[i].Id == e.Id)
                {
                    EntitetList[i].Valued = e.Valued;
                    return;
                }

            for (int i = 0; i < 12; i++)                   //Kao i liniiju
                if (Canvases[i].Taken && Canvases[i].Entitet != null && Canvases[i].Entitet.Id == e.Id)
                {
                    Canvases[i].Entitet = e;
                    return;
                }
        }
        public static ObservableCollection<Entitie> EntitetList { get; set; }     //Lista entiteta u koju dodajem one koje su u tabeli na prvom prozoru
        public static ObservableCollection<CanvasInfo> Canvases { get; set; }     //Kanvas na slici da bude ono ao greska i 
        public static ObservableCollection<Line> Lines { get; set; }
        public MyICommand<ListView> SelectionChangedCommand { get; set; }
        public MyICommand MouseLeftButtonUpCommand { get; set; }
        public MyICommand<Canvas> ButtonCommand { get; set; }
        public MyICommand<Canvas> DragOverCommand { get; set; }
        public MyICommand<Canvas> DropCommand { get; set; }
        public MyICommand<Canvas> MouseLeftButtonDownCommand { get; set; }
        public MyICommand AutoPlaceCommand { get; set; }
        public MyICommand HelpCommand { get; set; }
        public MyICommand ToggleToolTipsCommand { get; set; }

        bool toolTipsBool;
        public bool ToolTipsBool
        {
            get => toolTipsBool;
            set
            {
                toolTipsBool = value;
                MainWindowViewModel.UseToolTips = value;
                OnPropertyChanged("ToolTipsBool");
            }
        }

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
            }
        }
        bool dragging = false;
        Entitie selectedEntitet;
        public Entitie SelectedEntitet
        {
            get => selectedEntitet;
            set
            {
                selectedEntitet = value;
                OnPropertyChanged("SelectedEntitet");
            }

        }

        CanvasInfo currentCanvas;
        public CanvasInfo CurrentCanvas
        {
            get => currentCanvas;
            set
            {
                currentCanvas = value;
                OnPropertyChanged("CurrentCanvas");
            }
        }



        bool Cmp(CanvasInfo c)
        {
            if (CurrentCanvas == null || c == null) return false;
            return CurrentCanvas.Entitet == c.Entitet && CurrentCanvas.Taken == c.Taken && CurrentCanvas.Text == c.Text;
        }

        private void OnAutoPlace()
        {
            System.Diagnostics.Debug.WriteLine($"OnAutoPlace pozvan - EntitetList.Count: {EntitetList?.Count ?? 0}");
            
            if (EntitetList == null || EntitetList.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("EntitetList je null ili prazna");
                return;
            }
            
            List<Entitie> temp = new List<Entitie>();
            foreach (Entitie e in EntitetList)
            {
                System.Diagnostics.Debug.WriteLine($"Pokušavam da postavim entitet: {e?.Name ?? "NULL"}");
                for (int i = 0; i < 12; i++)
                {
                    if (!Canvases[i].Taken)
                    {
                        Canvases[i] = new CanvasInfo(e, true, i);
                        temp.Add(e);
                        System.Diagnostics.Debug.WriteLine($"Entitet {e.Name} postavljen na canvas {i}");
                        break;
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Ukupno postavljeno {temp.Count} entiteta");
            foreach (Entitie e in temp)
                EntitetList.Remove(e);
        }

        public NetworkDisplayViewModel()
        {
            // Prvo inicijalizuj Canvases
            if (Canvases == null)
            {
                Canvases = new ObservableCollection<CanvasInfo>();
                for (int i = 0; i < 12; i++)
                    Canvases.Add(new CanvasInfo(i));
            }
            
            // Zatim inicijalizuj EntitetList
            if (EntitetList == null)
            {
                EntitetList = new ObservableCollection<Entitie>();
                
                // Sinhronizuj sa entitetima iz NetworkEntitiesViewModel
                if (NetworkEntitiesViewModel.Entiteti != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Sinhronizujem {NetworkEntitiesViewModel.Entiteti.Count} entiteta iz NetworkEntitiesViewModel");
                    
                    foreach (Entitie e in NetworkEntitiesViewModel.Entiteti)
                    {
                        // Dodaj samo one koji nisu već na canvas-u
                        bool naCanvasu = false;
                        for (int i = 0; i < 12; i++)
                        {
                            if (Canvases[i].Taken && Canvases[i].Entitet != null && Canvases[i].Entitet.Id == e.Id)
                            {
                                naCanvasu = true;
                                break;
                            }
                        }
                        
                        if (!naCanvasu)
                        {
                            EntitetList.Add(e);
                            System.Diagnostics.Debug.WriteLine($"Sinhronizovan entitet {e.Name} u EntitetList");
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"EntitetList inicijalizovana sa {EntitetList.Count} entiteta");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("NetworkEntitiesViewModel.Entiteti je null");
                }
            }
            if (Lines == null)
                Lines = new ObservableCollection<Line>();

            DragOverCommand = new MyICommand<Canvas>(DragOver);
            DropCommand = new MyICommand<Canvas>(Drop);
            ButtonCommand = new MyICommand<Canvas>(ButtonCommandFreeing);
            SelectionChangedCommand = new MyICommand<ListView>(SelectionChanged);
            MouseLeftButtonUpCommand = new MyICommand(MouseLeftButtonUp);
            MouseLeftButtonDownCommand = new MyICommand<Canvas>(MouseLeftButtonDown);
            AutoPlaceCommand = new MyICommand(OnAutoPlace);
            HelpCommand = new MyICommand(OnHelp);
            ToggleToolTipsCommand = new MyICommand(OnToggleToolTips);
            helpText = saveHelp;
            ToolTipsBool = MainWindowViewModel.UseToolTips;
        }

        private void OnToggleToolTips()
        {
            ToolTipsBool = !ToolTipsBool;
        }

        private void OnHelp()
        {
            if (HelpText == "")
            {
                HelpText = "Prečice su sledeće:\nCTRL+D -> Automatsko stavljanje entiteta na mesta\nCtrl+H -> Help\nCtrl+T -> Toggle ToolTips\nCtrl+Tab pomjeranje izmedju prozora" +
                           "Prevlačenjem entiteta iz liste u odabrano polje će rezultirati prebacivanjem entiteta iz liste" +
                           " u to polje za prikaz trenutnog stanja tog entiteta.Prevlačenjem entiteta iz polja" +
                           " u polje ce rezultirati prebacivanjem entiteta iz polja u polje.\nPovlačenje linije" +
                           " izmedju 2 entiteta se radi povlačenjem prvog zauzetog polja na drugo polje.";
            }
            else
            {
                HelpText = "";
            }
        }

        void ChangeLine(int id, int x, int y, int nx, int ny)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Id == id)
                {
                    if (Lines[i].X1 == x && Lines[i].Y1 == y)
                    {
                        Lines[i].X1 = nx;
                        Lines[i].Y1 = ny;
                    }
                    else
                    {
                        Lines[i].X2 = nx;
                        Lines[i].Y2 = ny;
                    }
                    return;
                }
            }
        }

        private void Drop(Canvas obj)
        {
            System.Diagnostics.Debug.WriteLine($"Drop pozvan - obj.Name: {obj.Name}");
            
            if (SelectedEntitet != null)
            {
                int id = int.Parse(obj.Name.Substring(1));
                System.Diagnostics.Debug.WriteLine($"SelectedEntitet nije null, drop na canvas {id}, Taken: {Canvases[id].Taken}");
                
                if (!Canvases[id].Taken)
                {
                    Canvases[id] = new CanvasInfo(SelectedEntitet, true, id);
                    EntitetList.Remove(SelectedEntitet);
                    System.Diagnostics.Debug.WriteLine($"Entitet {SelectedEntitet?.Name ?? "NULL"} postavljen na canvas {id}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Canvas {id} je zauzet, ne mogu da postavim SelectedEntitet");
                }
            }
            else if (CurrentCanvas != null)
            {
                int id = int.Parse(obj.Name.Substring(1));
                System.Diagnostics.Debug.WriteLine($"CurrentCanvas nije null, drop na canvas {id}, Taken: {Canvases[id].Taken}");
                
                if (!Canvases[id].Taken)
                {
                    System.Diagnostics.Debug.WriteLine($"Pomeranje entiteta sa pozicije {CurrentCanvas.Id} na poziciju {id}");
                    for (int i = 0; i < 12; i++)
                        if (Cmp(Canvases[i]))
                        {
                            Canvases[i] = new CanvasInfo(i);
                            break;
                        }
                    Canvases[id] = new CanvasInfo(CurrentCanvas.Entitet, true, id);
                    foreach (int i in CurrentCanvas.Lines)
                    {
                        // Koordinate za ChangeLine isto idu do centra canvas-a
                        ChangeLine(i, CurrentCanvas.X + 100, CurrentCanvas.Y + 75, Canvases[id].X + 100, Canvases[id].Y + 75);
                        Canvases[id].Lines.Add(i);
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Pokušavam kreiranje linije između canvas {CurrentCanvas.Id} i {id}");
                    for (int i = 0; i < 12; i++)
                        if (Cmp(Canvases[i]))
                        {
                            // Proverava da li već postoji linija između ova dva canvas-a
                            bool lineExists = false;
                            foreach (int lineId in Canvases[i].Lines)
                            {
                                if (Canvases[id].Lines.Contains(lineId))
                                {
                                    lineExists = true;
                                    break;
                                }
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"Linija već postoji: {lineExists}");
                            
                            if (!lineExists)
                            {
                                // Koordinate linije idu do centra canvas-a (Canvas je 200x150, centar je +100, +75)
                                int x1 = Canvases[i].X + 100;
                                int y1 = Canvases[i].Y + 75;
                                int x2 = Canvases[id].X + 100;
                                int y2 = Canvases[id].Y + 75;
                                
                                Line line = new Line(x1, x2, y1, y2);
                                Lines.Add(line);
                                Canvases[i].Lines.Add(line.Id);
                                Canvases[id].Lines.Add(line.Id);
                                System.Diagnostics.Debug.WriteLine($"Kreirana linija ID={line.Id} između canvas {i}({x1},{y1}) i {id}({x2},{y2})");
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Linija već postoji između canvas {i} i {id}, neću kreirati duplikat");
                            }
                            break;
                        }
                }
            }
            MouseLeftButtonUp();
        }

        private void DragOver(Canvas obj)
        {
            int id = int.Parse(obj.Name.Substring(1));
            // Uvek dozvoli drop - ako je canvas slobodan, entitet će biti premešten
            // ako je zauzet, kreiraće se linija između entiteta
            obj.AllowDrop = true;
        }

        private void MouseLeftButtonUp()
        {
            SelectedEntitet = null;
            CurrentCanvas = null;
            dragging = false;
        }


        private void MouseLeftButtonDown(Canvas c)
        {
            int id = int.Parse(c.Name.Substring(1));
            if (Canvases[id].Taken)
            {
                CurrentCanvas = Canvases[id];
                if (!dragging)
                {
                    dragging = true;
                    DragDrop.DoDragDrop(c, CurrentCanvas, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
        }

        static void RemoveLine(int id)
        {
            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Id == id)
                {
                    Lines.RemoveAt(i);
                    return;
                }
            }
        }
        private void ButtonCommandFreeing(Canvas obj)
        {
            int id = int.Parse(obj.Name.Substring(1));
            System.Diagnostics.Debug.WriteLine($"FREE dugme pritisnuto za canvas {id}, Taken: {Canvases[id].Taken}");
            
            if (Canvases[id].Taken)
            {
                foreach (int i in Canvases[id].Lines)
                    RemoveLine(i);
                EntitetList.Add(Canvases[id].Entitet);
                System.Diagnostics.Debug.WriteLine($"Entitet {Canvases[id].Entitet?.Name ?? "NULL"} vraćen u listu. Lista sada ima {EntitetList.Count} entiteta");
                Canvases[id] = new CanvasInfo(id);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Canvas {id} nije zauzet, nema šta da se oslobodi");
            }
        }

        private void SelectionChanged(ListView obj)
        {
            // Postavi SelectedEntitet iz ListView SelectedItem
            if (obj.SelectedItem is Entitie selectedEntity)
            {
                SelectedEntitet = selectedEntity;
                System.Diagnostics.Debug.WriteLine($"SelectionChanged - SelectedEntitet postavljen na: {SelectedEntitet?.Name ?? "NULL"}");
                
                if (!dragging && SelectedEntitet != null)
                {
                    dragging = true;
                    DragDrop.DoDragDrop(obj, SelectedEntitet, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SelectionChanged - obj.SelectedItem nije Entitie ili je null");
                SelectedEntitet = null;
            }
        }
    }
}
