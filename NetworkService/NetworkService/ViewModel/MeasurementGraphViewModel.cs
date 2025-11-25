using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        public static GraphUpdaterG3 ElementRadii { get; set; } = new GraphUpdaterG3();
        private static int idForShow { get; set; } = -1;
        private static List<MeasurementGraphViewModel> AllInstances = new List<MeasurementGraphViewModel>();

        private string helpText;
        private bool toolTipsBool;
        private int selectedMeasurementId;
        private List<int> comboBoxData = new List<int>();

        public ObservableCollection<string> TimeLabels { get; set; } = new ObservableCollection<string>();

        public bool ToolTipsBool
        {
            get { return toolTipsBool; }
            set
            {
                toolTipsBool = value;
                MainWindowViewModel.UseToolTips = value;
                OnPropertyChanged("ToolTipsBool");
            }
        }

        public string HelpText
        {
            get => helpText;
            set
            {
                helpText = value;
                OnPropertyChanged("HelpText");
            }
        }

        public List<int> ComboBoxData
        {
            get => comboBoxData;
            set
            {
                comboBoxData = value;
                OnPropertyChanged("ComboBoxData");
            }
        }

        public int SelectedMeasurementId
        {
            get { return selectedMeasurementId; }
            set
            {
                if (selectedMeasurementId != value)
                {
                    selectedMeasurementId = value;
                    OnPropertyChanged("SelectedMeasurementId");
                    (ShowCommand as MyICommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand ShowCommand { get; set; }
        public ICommand HelpCommand { get; set; }
        public ICommand ToggleToolTipsCommand { get; set; }

        public MeasurementGraphViewModel()
        {
            AllInstances.Add(this);
            ToolTipsBool = MainWindowViewModel.UseToolTips;

            HelpCommand = new MyICommand(OnHelp);
            ShowCommand = new MyICommand(OnShow, CanShow);
            ToggleToolTipsCommand = new MyICommand(OnToggleToolTips);

            UpdateComboBoxData();
        }

        private void UpdateComboBoxData()
        {
            if (NetworkEntitiesViewModel.Entiteti != null)
            {
                ComboBoxData = NetworkEntitiesViewModel.Entiteti.Select(e => e.Id).ToList();
            }
            else
            {
                ComboBoxData = new List<int>();
            }
        }

        private bool CanShow()
        {
            return SelectedMeasurementId != 0;
        }

        private void OnShow()
        {
            var ent = NetworkEntitiesViewModel.Entiteti.FirstOrDefault(e => e.Id == SelectedMeasurementId);
            if (ent == null)
            {
                MessageBox.Show("Entitet sa izabranim ID-jem ne postoji.", "Greška", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            idForShow = SelectedMeasurementId;
            ElementRadii.ClearRadii();
            TimeLabels.Clear();

            // Odmah pokreće prvo ažuriranje sa trenutnom vrednošću
            OnIncomingValue(ent.Valued, ent.Id);
            TimeLabels.Add(DateTime.Now.ToString("HH:mm:ss"));
        }

        private void OnHelp()
        {
            if (string.IsNullOrEmpty(HelpText))
            {
                HelpText = "MEASUREMENT GRAPH - G3\n\n" +
                            "PREČICE:\n" +
                            "• CTRL+S → Prikaži graf za izabrani entitet\n" +
                            "• CTRL+H → Help\n" +
                            "• CTRL+T → Toggle ToolTips\n" +
                            "• CTRL+Tab → Navigacija između view-ova\n\n" +
                            "GRAF G3 - Krugovi razlicitih poluprecnika:\n" +
                            "• Prikazuje poslednjih 5 vrednosti\n" +
                            "• Plavi krugovi = validne vrednosti\n" +
                            "• Crveni krugovi = nevalidne vrednosti\n" +
                            "• Poluprečnik se skalira prema vrednosti\n" +
                            "• Vrednosti u krugovima, vreme na X-osi\n" +
                            "• Real-time ažuriranje sa Simulatorom\n\n" +
                            "T1 VALIDACIJA:\n" +
                            "• Vrednost mora biti između 5 i 16 MPa (MegaPaskala).";
            }
            else
            {
                HelpText = string.Empty;
            }
        }

        private void OnToggleToolTips()
        {
            ToolTipsBool = !ToolTipsBool;
        }

        public static void RefreshComboBoxData()
        {
            foreach (var vm in AllInstances)
            {
                vm.UpdateComboBoxData();
            }
        }

        public static void OnIncomingValue(double value, int entityId)
        {
            if (idForShow == entityId)
            {
                ElementRadii.FifthRadius = ElementRadii.FourthRadius;
                ElementRadii.FourthRadius = ElementRadii.ThirdRadius;
                ElementRadii.ThirdRadius = ElementRadii.SecondRadius;
                ElementRadii.SecondRadius = ElementRadii.FirstRadius;

                ElementRadii.FifthBrush = ElementRadii.FourthBrush;
                ElementRadii.FourthBrush = ElementRadii.ThirdBrush;
                ElementRadii.ThirdBrush = ElementRadii.SecondBrush;
                ElementRadii.SecondBrush = ElementRadii.FirstBrush;

                ElementRadii.FifthLabel = ElementRadii.FourthLabel;
                ElementRadii.FourthLabel = ElementRadii.ThirdLabel;
                ElementRadii.ThirdLabel = ElementRadii.SecondLabel;
                ElementRadii.SecondLabel = ElementRadii.FirstLabel;
                ElementRadii.FirstLabel = DateTime.Now.ToString("HH:mm:ss");

                ElementRadii.FirstRadius = CalculateElementRadius(value, entityId);
                UpdateBrushAndLabel(value, entityId);
            }
        }

        public static double CalculateElementRadius(double value, int entityId)
        {
            var ent = NetworkEntitiesViewModel.Entiteti.FirstOrDefault(e => e.Id == entityId);
            string typeName = ent?.Type?.Name ?? string.Empty;
            double maxRadius = 50;
            double normalizedValue = (value - 5) / (16 - 5);
            return normalizedValue * maxRadius;
        }

        public static void UpdateBrushAndLabel(double value, int entityId)
        {
            if (idForShow != entityId) return;
            bool valid = IsT1ValueValid(value);
            ElementRadii.FirstBrush = valid ? System.Windows.Media.Brushes.DodgerBlue : System.Windows.Media.Brushes.Red;
            ElementRadii.FirstLabel = value.ToString();
        }

        private static bool IsT1ValueValid(double value)
        {
            return value >= 5 && value <= 16;
        }

        public ObservableCollection<GraphPointForUI> GraphPoints { get; set; } = new ObservableCollection<GraphPointForUI>();

        private void UpdateGraphPoints()
        {
            GraphPoints.Clear();

            var radii = new double[]
            {
        ElementRadii.FirstRadius,
        ElementRadii.SecondRadius,
        ElementRadii.ThirdRadius,
        ElementRadii.FourthRadius,
        ElementRadii.FifthRadius
            };

            var brushes = new Brush[]
            {
        ElementRadii.FirstBrush,
        ElementRadii.SecondBrush,
        ElementRadii.ThirdBrush,
        ElementRadii.FourthBrush,
        ElementRadii.FifthBrush
            };

            double startX = 50;
            double gap = 80;

            for (int i = 0; i < 5; i++)
            {
                GraphPoints.Add(new GraphPointForUI
                {
                    Radius = radii[i],
                    Brush = brushes[i],
                    TimeLabel = TimeLabels[i],
                    XPosition = startX + i * gap
                });
            }
        }

    }
}