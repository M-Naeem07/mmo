﻿using MySqlConnector;
using System.Data.Common;
using System.Text;

namespace PersistenceServer
{
    public class DatabaseMysql : Database
    {
        public DatabaseMysql(SettingsReader settings) : base(settings)
        {
            ConnectionParams = $"Server={settings.MysqlHost};" +
                $"Port={settings.MysqlPort};" +
                $"Uid={settings.MysqlUser};" +
                $"Pwd={settings.MysqlPassword};" +
                $"Database={settings.MysqlDatabase}; Allow User Variables=True;";
            GetIdentitySqlCommand = "SELECT @@IDENTITY;";
        }

        protected override async Task<DbConnection> GetConnection(string parameters)
        {
            var connection = new MySqlConnection(parameters);
            await connection.OpenAsync();
            return connection;
        }

        protected override DbCommand GetCommand(string parameters, DbConnection? connection) => new MySqlCommand(parameters, (MySqlConnection?)connection);

        public override async Task CheckCreateDatabase(SettingsReader settings)
        {
            // command that checks if our database exists
            string cmdStr = $"SHOW DATABASES LIKE '{settings.MysqlDatabase}';";
            // special case for connection string: we don't specify the database, because it may not exist yet
            string firstTimeConnectionStr = $"Server={settings.MysqlHost};Port={settings.MysqlPort};Uid={settings.MysqlUser};Pwd={settings.MysqlPassword};";
            var doesDbExistQuery = await RunQuery(cmdStr, firstTimeConnectionStr);
            // if there is no database, create one
            if (!doesDbExistQuery.HasRows())
            {
                Console.Write("Database not found: creating... ");

                // create database
                string collation = settings.MysqlAccentSensitiveCollation ? "utf8mb4_0900_as_ci" : "utf8mb4_0900_ai_ci";
                cmdStr = $"CREATE DATABASE {settings.MysqlDatabase} CHARACTER SET utf8mb4 COLLATE {collation};";
                await RunNonQuery(cmdStr, firstTimeConnectionStr);
                // ~create database

                // create tables
                await RunNonQuery(
                    @"CREATE TABLE IF NOT EXISTS accounts (
	                    id int NOT NULL AUTO_INCREMENT,
	                    name varchar(50),
                        steamid varchar(20),
	                    password BINARY(60),
	                    salt BINARY(60),
	                    email varchar(255),
	                    status int,
	                    PRIMARY KEY (id),
	                    UNIQUE INDEX NAME (name),
                        UNIQUE INDEX STEAMID (steamid)
                    ) ENGINE = InnoDB;
                    CREATE TABLE IF NOT EXISTS guilds (
	                    id int NOT NULL AUTO_INCREMENT,
	                    name varchar(50) NOT NULL,
	                    PRIMARY KEY (id),
	                    UNIQUE INDEX NAME (name)
                    ) ENGINE = InnoDB;
                    CREATE TABLE IF NOT EXISTS characters (
	                    id int NOT NULL AUTO_INCREMENT,
	                    name varchar(50) NOT NULL,
                        owner int,
	                    guild int,
	                    guildrank int,
	                    serialized text,
	                    PRIMARY KEY (id),
	                    UNIQUE INDEX NAME (name),
                        INDEX OWNER (owner),
                        INDEX GUILD (guild),
                        CONSTRAINT character_owner_fk FOREIGN KEY (owner) REFERENCES accounts(id) ON UPDATE CASCADE ON DELETE SET NULL,
                        CONSTRAINT character_guild_fk FOREIGN KEY (guild) REFERENCES guilds(id) ON UPDATE CASCADE ON DELETE SET NULL
                    ) ENGINE = InnoDB;"
                );
                // ~create tables

                Console.WriteLine("done.");
            }
            else
            {                
                Console.WriteLine("Database found: " + doesDbExistQuery.GetString(0, 0));
            }
        }

        public override async Task<int> LoginUser(string accountName, string password)
        {
            var cmd = GetCommand("SELECT id, password, salt, status FROM accounts WHERE name = @accountName");
            cmd.AddParam("@accountName", accountName);
            var dt = await RunQuery(cmd);

            // if not account with this name is found
            if (!dt.HasRows())
            {
                return -1;
            }

            var id = (int)dt.GetInt(0, "id")!;
            var status = dt.GetInt(0, "status")!;
            var passwordInDb = Encoding.UTF8.GetString(dt.GetBinaryArray(0, "password"));
            var salt = Encoding.UTF8.GetString(dt.GetBinaryArray(0, "salt"));

            // if status is banned
            if (status == -1)
            {
                return -1;
            }

            // if wrong password
            if (passwordInDb != BCrypt.Net.BCrypt.HashPassword(password, salt + Pepper))
            {
                return -1;
            }

            // if everything checks out, allow login by returning user's id
            return id;
        }
    }
}
