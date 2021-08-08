using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using SqlDeploy.Config;

namespace SqlDeploy
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = LoadConfiguration();
            SqlServerConfig ssconf = config.GetSection("SqlServer").Get<SqlServerConfig>();
            SqlFolders sqlFolders = config.GetSection("SqlFolders").Get<SqlFolders>();
            string ssConnectionString = BuildConnectionString(ssconf);

            List<string> scripts = GetOrderedScriptPaths(sqlFolders);

            foreach (string s in scripts)
            {
                ExecuteSqlScript(s, ssConnectionString);
            }
        }

        private static void ExecuteSqlScript(string scriptPath, string connString)
        {
            string scriptText = File.ReadAllText(scriptPath);
            using (var connection = new SqlConnection(connString))
            {
                Console.WriteLine($"{scriptPath}");
                try
                {
                    connection.Execute(scriptText);
                }
                catch (Exception x)
                {
                    Console.WriteLine(x.Message);
                    throw;
                }
            }
        }

        private static List<string> GetOrderedScriptPaths(SqlFolders sqlFolders)
        {
            List<string> scripts = new List<string>();
            //goes through folders in the order they are in the app settings
            //and adds filepaths sorted alphabetically per folder.
            foreach (string folder in sqlFolders.SqlScripts)
            {
                try
                {
                    scripts.AddRange(Directory.GetFiles(Path.Combine(sqlFolders.SqlRoot, folder), "*.sql").ToList().OrderBy(name => name, StringComparer.InvariantCulture));
                }
                catch (DirectoryNotFoundException x)
                {
                    Console.WriteLine(x.Message);
                }
            }

            return scripts;
        }

        private static IConfiguration LoadConfiguration()
        {
            var confBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile("appsettings.Development.json", true)
                .AddEnvironmentVariables();

            return confBuilder.Build();
        }
        private static string BuildConnectionString(SqlServerConfig ssConfig)
        {
            return new SqlConnectionStringBuilder()
            {
                DataSource = ssConfig.DataSource,
                InitialCatalog = ssConfig.InitialCatalog,
                UserID = ssConfig.UserID,
                Password = ssConfig.Password
            }.ConnectionString;
        }
    }
}
