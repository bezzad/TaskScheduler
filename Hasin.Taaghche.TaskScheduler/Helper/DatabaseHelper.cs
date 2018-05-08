using System;
using System.IO;
using AdoManager;
using Dapper;
using NLog;

namespace Hasin.Taaghche.TaskScheduler.Helper
{
    public static class DatabaseHelper
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();


        public static bool IsExistDatabase(this ConnectionManager cm)
        {
            try
            {
                if (string.IsNullOrEmpty(cm.Connection.DatabaseName))
                {
                    var dbName = GetPathFromAttachDbFilename(cm.Connection.AttachDbFilename);

                    Logger.Info($"Checking the database file exist in this path: [{dbName}] ...");
                    // try to connect to database if can to connect that then is shown database is exist!
                    if (!File.Exists(dbName))
                    {
                        Logger.Info($"The [{Path.GetFileNameWithoutExtension(dbName)}] database was not found!");
                        return false;
                    }

                    cm.Open();
                    cm.Close();
                    Logger.Info(
                        $"Ok, Connected to [{Path.GetFileNameWithoutExtension(dbName)}] database successfully.");
                    return true;
                }

                Logger.Info($"Checking the [{cm.Connection.DatabaseName}] database is exist ...");
                // try to connect to database if can to connect that then is shown database is exist!
                cm.Open();
                cm.Close();
                Logger.Info($"Ok, Connected to [{cm.Connection.DatabaseName}] database successfully.");
                return true;
            }
            catch (Exception exp)
            {
                Logger.Error(exp);
                Logger.Info("The database was not found!");
                return false;
            }
        }

        public static bool CreateDatabase(this ConnectionManager cm)
        {
            try
            {
                Logger.Info("Trying to recreate the database ...");

                var server = cm.Connection.Server;

                using (var masterConn = new ConnectionManager(new Connection(server, "master")))
                {
                    if (string.IsNullOrEmpty(cm.Connection.AttachDbFilename)) // Sql Server
                    {
                        masterConn.SqlConn.Execute($"CREATE DATABASE {cm.Connection.DatabaseName}");
                        Logger.Info($"The database [{cm.Connection.DatabaseName}] created successfully.");
                        return IsExistDatabase(cm);
                    }

                    var dbFullPath = GetPathFromAttachDbFilename(cm.Connection.AttachDbFilename);

                    FileManager.CheckupDirectory(dbFullPath);

                    var dbName = Path.GetFileNameWithoutExtension(dbFullPath);
                    var dbLdfFullPath = Path.ChangeExtension(dbFullPath, ".ldf");

                    var sql = $@"CREATE DATABASE [{dbName}]
                                     ON PRIMARY (NAME={dbName?.Replace(" ", "_")}_data, FILENAME = '{dbFullPath}')
                                     LOG ON (NAME={dbName?.Replace(" ", "_")}_log, FILENAME = '{dbLdfFullPath}')";

                    masterConn.SqlConn.Execute(sql);

                    if (!File.Exists(dbFullPath)) return false;

                    // Detach database from Sql Server to used in this application only.
                    masterConn.DetachDatabase(dbName);

                    Logger.Info($"The [{dbName}] database created successfully.");
                    return true;
                }
            }
            catch (Exception exp)
            {
                Logger.Error(exp);
                return false;
            }
        }

        public static bool CreateDatabaseIfNotExist(this ConnectionManager cm, string continuesScript = null)
        {
            var result = cm.IsExistDatabase() || cm.CreateDatabase();

            try
            {
                if (!string.IsNullOrEmpty(continuesScript) && result)
                {
                    cm.SqlConn.Execute(continuesScript);
                    Logger.Info("The hangfire scripts executed on database successful.");
                }
            }
            catch (Exception exp)
            {
                Logger.Info("An exception occurred when execute the hangfire scripts.");
                Logger.Error(exp);
                result = false;
            }

            return result;
        }

        private static string GetPathFromAttachDbFilename(string attachDbFilename)
        {
            return attachDbFilename.Replace("|DataDirectory|",
                AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
        }

        public static bool DetachDatabase(this ConnectionManager cm, string dbName)
        {
            try
            {
                cm.Connection.DatabaseName = "master";
                cm.SqlConn.Execute($"exec sp_detach_db '{dbName}'");
                return true;
            }
            catch (Exception exp)
            {
                Logger.Error(exp);
                return false;
            }
        }
    }
}