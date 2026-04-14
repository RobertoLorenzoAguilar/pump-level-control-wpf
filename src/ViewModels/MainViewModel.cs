using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace PumpControl.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private double _currentLevel;
        private double _threshold;
        private string _pumpStatus;

        public double CurrentLevel
        {
            get => _currentLevel;
            set
            {
                _currentLevel = value;
                OnPropertyChanged();
                UpdateStatus();
            }
        }

        public double Threshold
        {
            get => _threshold;
            set
            {
                _threshold = value;
                OnPropertyChanged();
                UpdateStatus();
            }
        }

        public string PumpStatus
        {
            get => _pumpStatus;
            set
            {
                _pumpStatus = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            Threshold = 50;

            // Simulación de nivel dinámico
            var timer = new DispatcherTimer();
            timer.Interval = System.TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                CurrentLevel = (CurrentLevel + 5) % 100;
            };
            timer.Start();
        }

        private void UpdateStatus()
        {
            PumpStatus = CurrentLevel >= Threshold ? "ACTIVA" : "INACTIVA";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}