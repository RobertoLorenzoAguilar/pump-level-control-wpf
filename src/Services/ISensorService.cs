using System;

namespace PumpControl.Services
{
    public interface ISensorService
    {
        // Define que cualquier sensor debe poder darnos un valor decimal (0.0 a 100.0)
        double GetCurrentLevel();
        
        // Define un evento para avisar a la UI cuando cambie el dato
        event EventHandler<double> DataReceived;

        // Propaga una orden de control de regreso al hardware
        void SendCommand(string command);
    }
}