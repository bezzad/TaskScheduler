using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Http;
using AdoManager;
using Hangfire;
using Hangfire.Logging;
using Hangfire.Logging.LogProviders;
using Hangfire.SqlServer;
using Microsoft.Owin;
using NLog.Owin.Logging;
using Owin;
using TaskScheduler;
using TaskScheduler.Core;
using TaskScheduler.Helper;
using TaskScheduler.Properties;

[assembly: OwinStartup(typeof(Startup))]

namespace TaskScheduler
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var connString = Settings.Default.ServerConnectionString;
            if (Debugger.IsAttached)
                connString = Settings.Default.LocalConnectionString;

            #region Configure NLog middleware

            app.UseNLog();
            LogProvider.SetCurrentLogProvider(new ColouredConsoleLogProvider());

            #endregion

            #region Configure Web API

            // Configure Web API for self-host. 
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            app.UseWebApi(config);

            #endregion

            #region Configure Hangfire Background Worker

            // Configure AppDomain parameter to simplify the Configure – http://stackoverflow.com/a/3501950/1317575
            AppDomain.CurrentDomain.SetData("DataDirectory",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data"));

            var script = FileManager.ReadResourceFile(Settings.Default.HangfireDbScript);
            var cm = new ConnectionManager(new Connection(connString));
            cm.CreateDatabaseIfNotExist(script);

            GlobalConfiguration.Configuration.UseSqlServerStorage(connString,
                    new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(30) })
                .UseFilter(new LogEverythingAttribute());

            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                ServerCheckInterval = TimeSpan.FromSeconds(30),
                HeartbeatInterval = TimeSpan.FromSeconds(5)
                /*ServerName = "Hasin_Hangfire"*/
            }, JobStorage.Current);

            // Read and start jobs
            JobsManager.CheckupSetting(Settings.Default.SettingFileName, 30);

            #endregion

            // Alert all notify receivers to know TaskScheduler restarted
            if (JobsManager.Jobs?.Any() == true && JobsManager.Jobs[0].Notifications?.Any() == true)
            {
                foreach (var notify in JobsManager.Jobs[0].Notifications)
                {
                    notify.Notifying("`Application successful running...`", $"Task Scheduler v{GetType().Assembly.GetName().Version.ToString(3)} Restarted");
                }
            }
        }
    }
}