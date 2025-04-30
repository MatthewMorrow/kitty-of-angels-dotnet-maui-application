// Version 1.0
using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using KittyOfAngels.Database;

namespace KittyOfAngels.Services
{
    public class AppSettings(Database.Database database) : IAppSettings
    {
        private readonly Database.Database _database = database;

        public string? GetSetting(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                using var connection = _database.GetConnectionAsync().Result;
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Value FROM AppSetting WHERE Key = @Key";
                command.Parameters.AddWithValue("@Key", key);

                var result = command.ExecuteScalar();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving setting '{key}': {ex.Message}");
                return null;
            }
        }

        public void SaveSetting(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                using var connection = _database.GetConnectionAsync().Result;
                using var command = connection.CreateCommand();

                command.CommandText = "SELECT COUNT(*) FROM AppSetting WHERE Key = @Key";
                command.Parameters.AddWithValue("@Key", key);
                var exists = Convert.ToInt32(command.ExecuteScalar()) > 0;

                command.CommandText = !exists ? "INSERT INTO AppSetting (Key, Value, LastUpdated) VALUES (@Key, @Value, @LastUpdated)" :
                    "UPDATE AppSetting SET Value = @Value, LastUpdated = @LastUpdated WHERE Key = @Key";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Value", value);
                command.Parameters.AddWithValue("@LastUpdated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving setting '{key}': {ex.Message}");
            }
        }
    }
}