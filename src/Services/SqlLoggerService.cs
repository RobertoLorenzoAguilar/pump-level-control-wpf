using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace PumpControl.Services
{
    public class SqlLoggerService
    {
        // En un entorno de producción, esto vendría de un archivo appsettings.json o variable de entorno
        private readonly string _connectionString = "Server=192.168.1.50;Database=PumpDB;User Id=sa;Password=Brdk4@Bug7635S";

        public void LogEvent(string type, string desc, double level)
        {
            // Ejecutar en un hilo en segundo plano (Fire and Forget)
            // de esta manera, si la base de datos se tarda 15 segundos en responder
            // (o en fallar porque no está instalada), el WPF jamás se quedará trabado.
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        string query = "INSERT INTO EventLog (EventType, Description, CurrentLevel) VALUES (@type, @desc, @level)";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        
                        cmd.Parameters.AddWithValue("@type", type);
                        cmd.Parameters.AddWithValue("@desc", desc);
                        cmd.Parameters.AddWithValue("@level", level);

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        
                        Debug.WriteLine($"[BD LOG] {type}: {desc}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error al escribir en bitácora SQL: {ex.Message}");
                }
            });
        }
    }
}
