using System;
using System.IO;
using System.Data.SQLite;

namespace AIChess.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly string _databasePath;

        public DatabaseService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var aiChessPath = Path.Combine(appDataPath, "AIChess");
            
            if (!Directory.Exists(aiChessPath))
            {
                Directory.CreateDirectory(aiChessPath);
            }

            _databasePath = Path.Combine(aiChessPath, "AIChess.db");
            _connectionString = $"Data Source={_databasePath};Version=3;";
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public void InitializeDatabase()
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                var createGameHistoryTable = @"
                    CREATE TABLE IF NOT EXISTS GameHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        GameDate DATETIME NOT NULL,
                        WhitePlayer TEXT NOT NULL,
                        BlackPlayer TEXT NOT NULL,
                        Result TEXT NOT NULL,
                        Moves TEXT,
                        DurationMinutes INTEGER,
                        DifficultyLevel TEXT
                    )";

                var createGameStatsTable = @"
                    CREATE TABLE IF NOT EXISTS GameStats (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerName TEXT NOT NULL,
                        GamesWon INTEGER DEFAULT 0,
                        GamesLost INTEGER DEFAULT 0,
                        GamesDrawn INTEGER DEFAULT 0,
                        TotalGames INTEGER DEFAULT 0,
                        LastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP
                    )";

                using (var command = new SQLiteCommand(createGameHistoryTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SQLiteCommand(createGameStatsTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return connection.State == System.Data.ConnectionState.Open;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetDatabasePath()
        {
            return _databasePath;
        }
    }
}