using System;
using AIChess.Services;

namespace AIChess.Test
{
    class TestSQLite
    {
        static void Main(string[] args)
        {
            try
            {
                var dbService = new DatabaseService();
                Console.WriteLine($"Database path: {dbService.GetDatabasePath()}");
                
                Console.WriteLine("Initializing database...");
                dbService.InitializeDatabase();
                
                Console.WriteLine("Testing connection...");
                bool connected = dbService.TestConnection();
                
                if (connected)
                {
                    Console.WriteLine("✓ SQLite connection successful!");
                }
                else
                {
                    Console.WriteLine("✗ SQLite connection failed!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}