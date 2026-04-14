using System.ComponentModel;
using System.Runtime.CompilerServices;
using PumpControl.Services;

namespace PumpControl.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ISensorService _sensorService;
        private readonly SqlLoggerService _logger;
        private double _currentLevel;
        private double _threshold = 50.0; // Valor por defecto: 50%
        private string _pumpStatus = "Apagada";
        private string _lastStatus = "INIT";

        // Propiedad que la interfaz (XAML) va a "escuchar"
        public double CurrentLevel
        {
            get => _currentLevel;
            set { _currentLevel = value; OnPropertyChanged(); CheckPumpLogic(); }
        }

        public double Threshold
        {
            get => _threshold;
            set { _threshold = value; OnPropertyChanged(); }
        }

        public string PumpStatus
        {
            get => _pumpStatus;
            set { _pumpStatus = value; OnPropertyChanged(); }
        }

        public MainViewModel(ISensorService sensorService, SqlLoggerService logger = null)
        {
            _sensorService = sensorService;
            _logger = logger ?? new SqlLoggerService(); // Por defecto lo creamos si no se inyecta
            
            // Nos suscribimos al evento del servicio
            _sensorService.DataReceived += (s, level) => 
            {
                // Importante: Actualizamos el nivel (WPF se encarga del resto)
                CurrentLevel = level;
            };
        }

        private void CheckPumpLogic()
        {
            if (CurrentLevel < Threshold)
            {
                PumpStatus = "ENCENDIDA (Llenando...)";
                
                if (_lastStatus != "ON")
                {
                    _logger.LogEvent("ALERTA", "Bomba Activada por nivel bajo debajo del umbral.", CurrentLevel);
                    _lastStatus = "ON";
                }
                // Aquí podrías llamar a un método en el servicio para enviar "ON" al Arduino
            }
            else
            {
                PumpStatus = "Apagada (Nivel OK)";
                
                if (_lastStatus != "OFF" && _lastStatus != "INIT")
                {
                    _logger.LogEvent("INFO", "Nivel recuperado. Bomba Desactivada.", CurrentLevel);
                    _lastStatus = "OFF";
                }
                else if (_lastStatus == "INIT")
                {
                    _lastStatus = "OFF";
                }
                // Aquí enviarías "OFF" al Arduino
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}