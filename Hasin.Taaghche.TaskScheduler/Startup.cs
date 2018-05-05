using System;
using System.IO;
using System.Web.Http;
using AdoManager;
using Owin;
using Hangfire;
using Hangfire.Logging;
using Hangfire.Logging.LogProviders;
using Hangfire.SqlServer;
using Hasin.Taaghche.TaskScheduler;
using Hasin.Taaghche.TaskScheduler.Core;
using Hasin.Taaghche.TaskScheduler.Helper;
using Microsoft.Owin;
using NLog.Owin.Logging;

[assembly: OwinStartup(typeof(Startup))]

namespace Hasin.Taaghche.TaskScheduler
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            #region Config NLog middleware

            app.UseNLog();
            LogProvider.SetCurrentLogProvider(new ColouredConsoleLogProvider());

            #endregion

            #region Configure Web API

            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);

            #endregion

            #region Config Hangfire Background Worker

            // Configure AppDomain parameter to simplify the config – http://stackoverflow.com/a/3501950/1317575
            AppDomain.CurrentDomain.SetData("DataDirectory",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data"));

            var script = FileManager.ReadResourceFile(Properties.Settings.Default.HangfireDbScript);
            var cm = new ConnectionManager(new Connection(Properties.Settings.Default.ServerConnectionString));
            cm.CreateDatabaseIfNotExist(script);

            GlobalConfiguration.Configuration.UseSqlServerStorage(
                    Properties.Settings.Default.ServerConnectionString
                    , new SqlServerStorageOptions {QueuePollInterval = TimeSpan.FromSeconds(5)})
                .UseFilter(new LogEverythingAttribute());


            // Read and start jobs
            JobsManager.CheckupSetting(Properties.Settings.Default.SettingFileName, 5);

            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer(new BackgroundJobServerOptions()
            {
                ServerCheckInterval = TimeSpan.FromSeconds(5),
                HeartbeatInterval = TimeSpan.FromSeconds(5)
                /*ServerName = "Hasin_Hangfire"*/
            }, JobStorage.Current);

            #endregion
        }
    }
}