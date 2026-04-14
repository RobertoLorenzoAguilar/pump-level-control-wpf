using System;

namespace PumpControl.Models
{
    /// <summary>
    /// Entidad (Model) abstracta que representa el estado operacional y parámetros del tanque.
    /// En una arquitectura estricta MVVM, el ViewModel envuelve a esta clase para separarla de la capa de acceso y la Vista.
    /// </summary>
    public class TankData
    {
        // Nivel físico actual del sistema de 0.0 a 100.0 %
        public double CurrentLevel { get; set; }
        
        // Cota de trabajo/SetPoint 
        public double TargetThreshold { get; set; }
        
        // Estado inferido del actuador mecánico
        public bool IsPumpActive { get; set; }
        
        // Marca temporal de registro o telemetría
        public DateTime LastUpdated { get; set; }

        public TankData()
        {
            // Valores iniciales por defecto (Safe State)
            CurrentLevel = 0.0;
            TargetThreshold = 50.0;
            IsPumpActive = false;
            LastUpdated = DateTime.Now;
        }
    }
}
