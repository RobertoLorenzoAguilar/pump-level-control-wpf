using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace PumpControl.Services
{
    public class SqlLoggerService
    {
        // En un entorno de producción, esto vendría de un archivo appsettings.json o variable de entorno
        private readonly string _connectionString = "Server=localhost\\SQLEXPRESS;Database=PumpDB;Trusted_Connection=True;";

        public void LogEvent(string type, string desc, double level)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    string query = "INSERT INTO EventLog (EventType, Description, CurrentLevel) VALUES (@type, @desc, @level)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    
                    // Asegurarse de prevenir SQL Injection con parámetros
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
                // Para el MVP y no detener la aplicación asumiendo que la BD podría no existir aún en Local
                Debug.WriteLine($"Error al escribir en bitácora SQL: {ex.Message}");
            }
        }
    }
}
