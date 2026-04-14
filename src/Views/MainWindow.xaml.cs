using System.Windows;
using PumpControl.Services;
using PumpControl.ViewModels;
using System.Diagnostics;

namespace PumpControl.Views
{
    public partial class MainWindow : Window
    {
        private SerialSensorService _serialService;

        public MainWindow()
        {
            InitializeComponent();
            
            // Inicializar el puerto serial y el ViewModel
            // Nota: En producción esto usualmente se inyecta por Inyección de Dependencias
            _serialService = new SerialSensorService("COM3", 9600);
            
            // Intenta abrir el puerto, si Arduino no está fallará de forma controlada
            _serialService.Open();
            
            // Asigna el DataContext para enlazar los datos (Binding) al Frontend
            // Se inyectan ambos servicios (Hardware Sensor y Base de Datos SQL)
            var loggerService = new SqlLoggerService();
            this.DataContext = new MainViewModel(_serialService, loggerService);
            // Asegurarse de liberar recursos del puerto al cerrar la misma ventana
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_serialService != null)
            {
                _serialService.Dispose();
            }
        }
    }
}
