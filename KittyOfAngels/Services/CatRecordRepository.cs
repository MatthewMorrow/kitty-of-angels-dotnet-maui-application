// Version 1.0
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Diagnostics;

namespace KittyOfAngels.Services
{
    public class CatRecordRepository(Database.Database database)
    {
        public async Task<bool> AddCatRecordAsync(string internalId)
        {
            try
            {
                await using var connection = await database.GetConnectionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = @"
                    INSERT OR IGNORE INTO CatRecord (InternalId, LastViewed, SyncStatus)
                    VALUES (@InternalId, @LastViewed, @SyncStatus)";

                command.Parameters.AddWithValue("@InternalId", internalId);
                command.Parameters.AddWithValue("@LastViewed", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@SyncStatus", "Synced");

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding cat record: {ex.Message}");
                return false;
            }
        }

        public async Task<List<string>> GetAllCatRecordsAsync()
        {
            var catIds = new List<string>();

            try
            {
                await using var connection = await database.GetConnectionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = "SELECT InternalId FROM CatRecord";

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var internalId = reader.GetString(0);
                    catIds.Add(internalId);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting cat records: {ex.Message}");
            }

            return catIds;
        }

        public async Task<bool> UpdateCatRecordAsync(string internalId, string syncStatus)
        {
            try
            {
                await using var connection = await database.GetConnectionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = @"
                    UPDATE CatRecord 
                    SET LastViewed = @LastViewed, SyncStatus = @SyncStatus
                    WHERE InternalId = @InternalId";

                command.Parameters.AddWithValue("@InternalId", internalId);
                command.Parameters.AddWithValue("@LastViewed", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                command.Parameters.AddWithValue("@SyncStatus", syncStatus);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating cat record: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteCatRecordAsync(string internalId)
        {
            try
            {
                await using var connection = await database.GetConnectionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = "DELETE FROM CatRecord WHERE InternalId = @InternalId";
                command.Parameters.AddWithValue("@InternalId", internalId);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting cat record: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CatRecordExistsAsync(string internalId)
        {
            try
            {
                await using var connection = await database.GetConnectionAsync();
                await using var command = connection.CreateCommand();

                command.CommandText = "SELECT COUNT(1) FROM CatRecord WHERE InternalId = @InternalId";
                command.Parameters.AddWithValue("@InternalId", internalId);

                var count = (long)await command.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking if cat record exists: {ex.Message}");
                return false;
            }
        }
    }
}