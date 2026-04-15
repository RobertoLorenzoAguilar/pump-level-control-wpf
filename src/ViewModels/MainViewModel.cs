using System;
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

        // ─── CALIBRACIÓN FÍSICA ───────────────────────────────────────────────────
        // El sensor YL-69 alcanza saturación física a este % del rango ADC.
        // Por encima de este valor, el tanque se considera lleno al 100%.
        private const double MaxPhysicalLevel = 5.0;

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
                OnPropertyChanged(nameof(DisplayLevel));
                OnPropertyChanged(nameof(FillStatusText));
                UpdateStatus();
            }
        }

        /// <summary>
        /// Nivel escalado para visualización: mapea el rango físico real (0–MaxPhysicalLevel %)
        /// al rango de pantalla completo (0–100 %). Es lo que ve el operador en la UI.
        /// Ej: sensor al 5 % raw → DisplayLevel = 100 % (tanque lleno).
        /// </summary>
        public double DisplayLevel
            => Math.Min(100.0, Math.Round((_currentLevel / MaxPhysicalLevel) * 100.0, 1));

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

        /// <summary>
        /// Texto descriptivo del estado de llenado. Trabaja sobre DisplayLevel (escala 0–100 %).
        /// </summary>
        public string FillStatusText
        {
            get
            {
                double dl        = DisplayLevel;
                double remaining = Math.Round(100.0 - dl, 1);
                if (dl >= 98.0)
                    return "\u2705 Nivel m\u00e1ximo alcanzado \u2014 Bomba apagada";
                if (dl >= 70.0)
                    return $"\u25b2 Nivel elevado \u2014 Falta {remaining:F1}\u202f% para llenarse";
                if (dl >= 30.0)
                    return $"\u26a0 Nivel medio \u2014 Falta {remaining:F1}\u202f% para llenarse";
                return $"\ud83d\udd34 NIVEL CR\u00cdTICO \u2014 Falta {remaining:F1}\u202f% para llenarse";
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
            Threshold = 80;  // 80 % de DisplayLevel = punto de corte operativo
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
            if (IsManualMode) return; // En manual, las reglas automáticas no aplican.

            // Comparar contra DisplayLevel (escala 0-100 %) para que el Threshold
            // del slider coincida con lo que ve el operador en pantalla.
            bool isTriggered = DisplayLevel >= Threshold;
            string newStatus = isTriggered ? "ACTIVA" : "INACTIVA";

            // Evita rearmar el estado si ya estamos en el mismo
            if (PumpStatus == newStatus) return;

            PumpStatus = newStatus;
            string hwCommand = isTriggered ? "1" : "0";

            _sensorService?.SendCommand(hwCommand);

            // Log con nivel crudo (valor de ingeniería) y nivel normalizado para auditoría completa
            _logger?.LogEvent(
                isTriggered ? "PUMP_ON" : "PUMP_OFF",
                $"DisplayLevel={DisplayLevel:F1}% (raw={CurrentLevel:F2}%) cruzó umbral {Threshold}%",
                DisplayLevel);
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