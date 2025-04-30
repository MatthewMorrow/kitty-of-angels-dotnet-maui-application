// Version 1.0
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Storage;

namespace KittyOfAngels.Database
{
    public class Database
    {
        private readonly string _databasePath;
        private readonly string _connectionString;

        public Database()
        {
            const string databaseName = "kittyofangels.db";
            _databasePath = Path.Combine(FileSystem.AppDataDirectory, databaseName);
            _connectionString = $"Data Source={_databasePath}";
            InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task InitializeAsync()
        {
            if (!File.Exists(_databasePath))
            {
                await CreateDatabaseAsync();
            }
        }

        private async Task CreateDatabaseAsync()
        {
            await using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var createAppSettingTableCommand = connection.CreateCommand();
            createAppSettingTableCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS AppSetting (
                    Key TEXT PRIMARY KEY,
                    Value TEXT,
                    LastUpdated TEXT
                )";
            await createAppSettingTableCommand.ExecuteNonQueryAsync();

            var createCatRecordCommand = connection.CreateCommand();
            createCatRecordCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS CatRecord (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    InternalId TEXT UNIQUE NOT NULL,
                    LastViewed TEXT,
                    SyncStatus TEXT
                )";
            await createCatRecordCommand.ExecuteNonQueryAsync();
        }

        public async Task<SqliteConnection> GetConnectionAsync()
        {
            var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}