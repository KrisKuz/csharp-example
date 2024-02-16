// Program.cs
using System;
using System.Data.SqlClient;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            string server = "localhost,1433"; // Make sure your SQL Server is running on this address
            string password = "Password1!";
            
            string databaseName = "YourDatabase";
            string tableName = "TestTable";
            
            string loginName = "YourLogin";
            string userPassword = "YourUserPassword1!";

            // Master Connection String
            string masterConnectionString = $"Server={server};Database=master;User Id=sa;Password={password};";

            using (SqlConnection masterConnection = new SqlConnection(masterConnectionString))
            {
                masterConnection.Open();

                // Create database if it doesn't exist
                string createDatabaseQuery = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}') " +
                                             $"CREATE DATABASE [{databaseName}]";
                SqlCommand command = new SqlCommand(createDatabaseQuery, masterConnection);
                command.ExecuteNonQuery();

                // Create login if it doesn't exist
                string createLoginQuery = $"IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = N'{loginName}') " +
                                          $"CREATE LOGIN [{loginName}] WITH PASSWORD = N'{userPassword}';";
                command = new SqlCommand(createLoginQuery, masterConnection);
                command.ExecuteNonQuery();
            }

            // Database Connection String
            // It's important to connect AFTER creating the database
            string databaseConnectionString = $"Server={server};Database={databaseName};User Id=sa;Password={password};";

            using (SqlConnection databaseConnection = new SqlConnection(databaseConnectionString))
            {
                databaseConnection.Open();

                // Create user for the login if it doesn't exist
                string createUserQuery = $"IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = N'{loginName}') " +
                                         $"CREATE USER [{loginName}] FOR LOGIN [{loginName}]";
                SqlCommand command = new SqlCommand(createUserQuery, databaseConnection);
                command.ExecuteNonQuery();

                // Assign appropriate permissions for the created user
                // For example, make the user db_owner which gives all permissions on the database
                string assignDbRole = $"ALTER ROLE db_owner ADD MEMBER [{loginName}];";
                command = new SqlCommand(assignDbRole, databaseConnection);
                command.ExecuteNonQuery();
            }
            
            
            
            
            using (SqlConnection connection = new SqlConnection(databaseConnectionString))
            {
                connection.Open();
            
                // Create table if it doesn't exist
                string createTableQuery = $"IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{tableName}') " +
                                          $"CREATE TABLE {tableName} (Id INT IDENTITY(1,1), Name NVARCHAR(50))";
                using (SqlCommand command = new SqlCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            
                // Insert a row
                string insertRowQuery = $"INSERT INTO {tableName} (Name) VALUES ('Test Name')";
                using (SqlCommand command = new SqlCommand(insertRowQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            
                // Retrieve all rows
                string selectAllRowsQuery = $"SELECT * FROM {tableName}";
                using (SqlCommand command = new SqlCommand(selectAllRowsQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Id: {reader["Id"]}, Name: {reader["Name"]}");
                        }
                    }
                }
            }
        }
    }
}