using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PumpControl.Services;

namespace PumpControl.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ISensorService _sensorService;
        private SqlLoggerService _logger;

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

        private bool _isManualMode;
        public bool IsManualMode
        {
            get => _isManualMode;
            set
            {
                _isManualMode = value;
                OnPropertyChanged();
                if (!value) UpdateStatus(); // Al volver a auto, reevaluar.
            }
        }

        public System.Windows.Input.ICommand ToggleManualCommand { get; }
        public System.Windows.Input.ICommand ForceOnCommand { get; }
        public System.Windows.Input.ICommand ForceOffCommand { get; }

        public MainViewModel()
        {
            Threshold = 50;
            _logger = new SqlLoggerService();

            // Comandos manuales
            ToggleManualCommand = new RelayCommand(() => IsManualMode = !IsManualMode);
            ForceOnCommand = new RelayCommand(() => 
            {
                if (!IsManualMode) return;
                PumpStatus = "ACTIVA (MANUAL)";
                _sensorService?.SendCommand("1");
                _logger?.LogEvent("PUMP_ON_MANUAL", "Bomba encendida manualmente desde UI", CurrentLevel);
            });
            ForceOffCommand = new RelayCommand(() => 
            {
                if (!IsManualMode) return;
                PumpStatus = "INACTIVA (MANUAL)";
                _sensorService?.SendCommand("0");
                _logger?.LogEvent("PUMP_OFF_MANUAL", "Bomba apagada manualmente desde UI", CurrentLevel);
            });

            // Instancia el Hardware real en el puerto asignado (COM12)
            var serialService = new SerialSensorService("COM12", 9600);
            _sensorService = serialService;

            // Recibir datos asíncronos del USB
            _sensorService.DataReceived += (s, level) =>
            {
                // Al venir del hilo serial, lo mandamos al hilo principal de UI WPF
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentLevel = level;
                });
            };

            // Iniciar protocolo de comunicación
            serialService.Open();
        }

        private void UpdateStatus()
        {
            if (IsManualMode) return; // Si estamos en manual, no aplicar reglas automáticas.

            bool isTriggered = CurrentLevel >= Threshold;
            string newStatus = isTriggered ? "ACTIVA" : "INACTIVA";
            
            // ¡Evita atascar (trabar) el hilo comprobando si ya estábamos en este mismo estado!
            if (PumpStatus == newStatus) return;
            
            PumpStatus = newStatus;
            string hwCommand = isTriggered ? "1" : "0";
            
            _sensorService?.SendCommand(hwCommand);
            
            // Intentará grabar en la BD. Si no existe, The task envolverá la caída en silencio.
            _logger?.LogEvent(isTriggered ? "PUMP_ON" : "PUMP_OFF", 
                $"Nivel cruzó la línea marcando {CurrentLevel}% (Umbral: {Threshold}%)", CurrentLevel);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Pequeño helper para usar ICommand sin MVVM toolkits pesados
    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly System.Action _execute;
        public RelayCommand(System.Action execute) { _execute = execute; }
        public event System.EventHandler CanExecuteChanged { add { } remove { } }
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
    }
}