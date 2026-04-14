using System;
using System.IO.Ports; // Necesitas agregar la referencia en NuGet
using System.Diagnostics;

namespace PumpControl.Services
{
    public class SerialSensorService : ISensorService, IDisposable
    {
        private SerialPort _serialPort;
        private readonly string _portName;
        private readonly int _baudRate;
        
        private double _lastLevel = 0.0;

        // Evento definido en la interfaz
        public event EventHandler<double> DataReceived;

        public SerialSensorService(string portName = "COM12", int baudRate = 9600)
        {
            _portName = portName;
            _baudRate = baudRate;
            SetupSerialPort();
        }

        private void SetupSerialPort()
        {
            _serialPort = new SerialPort(_portName, _baudRate);
            
            // Suscripción al evento de recepción de datos del hardware
            _serialPort.DataReceived += SerialPort_DataReceived;

            string punto = "test";
        }

        public void Open()
        {
            try {
                if (!_serialPort.IsOpen) _serialPort.Open();
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error abriendo puerto: {ex.Message}");
            }
        }
        
        // --- MÉTODO AGREGADO ---
        // Implementación requerida por el contrato de la interfaz ISensorService
        public double GetCurrentLevel()
        {
            return _lastLevel;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try 
            {
                // Leemos la línea que manda el Arduino (ej: "450")
                string rawData = _serialPort.ReadLine();
                
                if (int.TryParse(rawData.Trim(), out int adcValue))
                {
                    // Invertimos la lógica matemática: 1023 (Seco) = 0%, 0 (Mojado) = 100%
                    double percentage = 100.0 - ((adcValue / 1023.0) * 100.0);
                    
                    // Almacenamos internamente el nivel más reciente detectado
                    _lastLevel = Math.Round(percentage, 2);

                    // Disparamos el evento para que el ViewModel se entere
                    DataReceived?.Invoke(this, _lastLevel);
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error en lectura: {ex.Message}");
            }
        }

        public void SendCommand(string command)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Write(command);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al enviar comando: {ex.Message}");
            }
        }

        public void Close() => _serialPort?.Close();

        public void Dispose()
        {
            Close();
            _serialPort?.Dispose();
        }
    }
}