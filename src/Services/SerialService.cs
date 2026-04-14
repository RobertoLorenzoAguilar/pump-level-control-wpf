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

        // Evento definido en la interfaz
        public event EventHandler<double> DataReceived;

        public SerialSensorService(string portName = "COM3", int baudRate = 9600)
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

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try 
            {
                // Leemos la línea que manda el Arduino (ej: "450")
                string rawData = _serialPort.ReadLine();
                
                if (int.TryParse(rawData.Trim(), out int adcValue))
                {
                    // Convertimos ADC (0-1023) a Porcentaje (0-100)
                    double percentage = (adcValue / 1023.0) * 100;

                    // Disparamos el evento para que el ViewModel se entere
                    DataReceived?.Invoke(this, Math.Round(percentage, 2));
                }
            }
            catch (Exception ex) {
                Debug.WriteLine($"Error en lectura: {ex.Message}");
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