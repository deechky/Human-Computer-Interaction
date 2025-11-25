using NetworkService.Helpers;
using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NetworkService.ViewModel
{
    public class GraphUpdaterG3 : BindableBase
    {
        private ObservableCollection<Entitie> measurements = new ObservableCollection<Entitie>();
        private ObservableCollection<string> timeLabels = new ObservableCollection<string>();

        private double firstRadius;
        private double secondRadius;
        private double thirdRadius;
        private double fourthRadius;
        private double fifthRadius;

        private Brush firstBrush = Brushes.Gray;
        private Brush secondBrush = Brushes.Gray;
        private Brush thirdBrush = Brushes.Gray;
        private Brush fourthBrush = Brushes.Gray;
        private Brush fifthBrush = Brushes.Gray;

        private string firstLabel = string.Empty;
        private string secondLabel = string.Empty;
        private string thirdLabel = string.Empty;
        private string fourthLabel = string.Empty;
        private string fifthLabel = string.Empty;

        public double FirstRadius
        {
            get { return firstRadius; }
            set { SetProperty(ref firstRadius, value); }
        }

        public double SecondRadius
        {
            get { return secondRadius; }
            set { SetProperty(ref secondRadius, value); }
        }

        public double ThirdRadius
        {
            get { return thirdRadius; }
            set { SetProperty(ref thirdRadius, value); }
        }

        public double FourthRadius
        {
            get { return fourthRadius; }
            set { SetProperty(ref fourthRadius, value); }
        }

        public double FifthRadius
        {
            get { return fifthRadius; }
            set { SetProperty(ref fifthRadius, value); }
        }

        public Brush FirstBrush
        {
            get { return firstBrush; }
            set { SetProperty(ref firstBrush, value); }
        }

        public Brush SecondBrush
        {
            get { return secondBrush; }
            set { SetProperty(ref secondBrush, value); }
        }

        public Brush ThirdBrush
        {
            get { return thirdBrush; }
            set { SetProperty(ref thirdBrush, value); }
        }

        public Brush FourthBrush
        {
            get { return fourthBrush; }
            set { SetProperty(ref fourthBrush, value); }
        }

        public Brush FifthBrush
        {
            get { return fifthBrush; }
            set { SetProperty(ref fifthBrush, value); }
        }

        public string FirstLabel
        {
            get { return firstLabel; }
            set { SetProperty(ref firstLabel, value); }
        }

        public string SecondLabel
        {
            get { return secondLabel; }
            set { SetProperty(ref secondLabel, value); }
        }

        public string ThirdLabel
        {
            get { return thirdLabel; }
            set { SetProperty(ref thirdLabel, value); }
        }

        public string FourthLabel
        {
            get { return fourthLabel; }
            set { SetProperty(ref fourthLabel, value); }
        }

        public string FifthLabel
        {
            get { return fifthLabel; }
            set { SetProperty(ref fifthLabel, value); }
        }

        public ObservableCollection<string> TimeLabels
        {
            get { return timeLabels; }
            set { timeLabels = value; OnPropertyChanged("TimeLabels"); }
        }

        public ObservableCollection<Entitie> Measurements
        {
            get { return measurements; }
            set { measurements = value; OnPropertyChanged("Measurements"); }
        }

        public void AddMeasurement(Entitie ent)
        {
            if (measurements.Count >= 5)
            {
                measurements.RemoveAt(0);
                timeLabels.RemoveAt(0);
            }

            measurements.Add(ent);
            timeLabels.Add(DateTime.Now.ToString("HH:mm:ss"));
            UpdateGraphData();
        }

        public void ChangeSelectedItem(Entitie selected)
        {
            Measurements.Clear();
            TimeLabels.Clear();
            UpdateGraphData();
        }

        private void UpdateGraphData()
        {
            ClearRadii();

            for (int i = 0; i < measurements.Count; i++)
            {
                double value = measurements[i].Valued;

                double radius = value * 10;
                Brush brush;
                if (value > 4.5 || value < 3.5)
                {
                    brush = Brushes.Red;
                }
                else
                {
                    brush = Brushes.DodgerBlue;
                }
                string label = value.ToString();

                switch (i)
                {
                    case 0:
                        FirstRadius = radius;
                        FirstBrush = brush;
                        FirstLabel = label;
                        break;
                    case 1:
                        SecondRadius = radius;
                        SecondBrush = brush;
                        SecondLabel = label;
                        break;
                    case 2:
                        ThirdRadius = radius;
                        ThirdBrush = brush;
                        ThirdLabel = label;
                        break;
                    case 3:
                        FourthRadius = radius;
                        FourthBrush = brush;
                        FourthLabel = label;
                        break;
                    case 4:
                        FifthRadius = radius;
                        FifthBrush = brush;
                        FifthLabel = label;
                        break;
                }
            }
        }

        public void ClearRadii()
        {
            FirstRadius = 0;
            SecondRadius = 0;
            ThirdRadius = 0;
            FourthRadius = 0;
            FifthRadius = 0;

            FirstBrush = Brushes.Gray;
            SecondBrush = Brushes.Gray;
            ThirdBrush = Brushes.Gray;
            FourthBrush = Brushes.Gray;
            FifthBrush = Brushes.Gray;

            FirstLabel = string.Empty;
            SecondLabel = string.Empty;
            ThirdLabel = string.Empty;
            FourthLabel = string.Empty;
            FifthLabel = string.Empty;
        }
    }
}
