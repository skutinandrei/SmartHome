using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome
{
    public class LogService
    {
        private readonly string _connectionString;

        public LogService(string connectionString = "Data Source=user_actions.db")
        {
            _connectionString = connectionString;
        }

        public async Task LogActionAsync(long userId, string deviceId, string actionType, string parameters)
        {
            const string sql = @"
            INSERT INTO UserActionLogs 
            (UserId, Timestamp, DeviceId, ActionType, Parameters) 
            VALUES 
            (@UserId, @Timestamp, @DeviceId, @ActionType, @Parameters)";

            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(sql, new
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                DeviceId = deviceId,
                ActionType = actionType,
                Parameters = parameters
            });
        }

        public async Task EnsureDatabaseCreatedAsync()
        {
            const string sql = @"
            CREATE TABLE IF NOT EXISTS UserActionLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                Timestamp DATETIME NOT NULL,
                DeviceId TEXT NOT NULL,
                ActionType TEXT NOT NULL,
                Parameters TEXT NOT NULL
            )";

            using var connection = new SqliteConnection(_connectionString);
            await connection.ExecuteAsync(sql);
        }

        public async Task<IEnumerable<UserActionLog>> GetLastLogsAsync(int count = 10)
        {
            const string sql = @"
            SELECT * 
            FROM UserActionLogs 
            ORDER BY Timestamp DESC 
            LIMIT @Count";

            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<UserActionLog>(sql, new { Count = count });
        }
    }
}
