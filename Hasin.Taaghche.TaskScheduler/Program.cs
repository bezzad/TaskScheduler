using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using NLog;
using Topshelf;

namespace Hasin.Taaghche.TaskScheduler
{
    public class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            var exitCode = HostFactory.Run(configurator =>
            {
                var version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
                configurator.UseNLog(Logger.Factory);
                configurator.RunAsLocalSystem();
                configurator.SetDisplayName($"Task Scheduler v{version}");
                configurator.SetDescription("Manage and execute actions by Hangfire job runners.");
                configurator.SetServiceName("TaaghcheTaskScheduler");
                configurator.SetStartTimeout(TimeSpan.FromMinutes(5));
                configurator.OnException(exception =>
                {
                    LogManager.GetCurrentClassLogger().Fatal(exception);
                    Debugger.Break();
                });
                configurator.Service<Service>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(settings => new Service(settings, 8003));
                    serviceConfigurator.WhenStarted((service) => service.Start());
                    serviceConfigurator.WhenStopped((service) => service.Stop());
                });

                configurator.EnableServiceRecovery(r =>
                {
                    //you can have up to three of these
                    r.RestartService(1);

                    //should this be true for crashed or non-zero exits
                    r.OnCrashOnly();

                    //number of days until the error count resets
                    //r.SetResetPeriod(1); // set the reset interval to one day
                });
            });
            if (exitCode == TopshelfExitCode.Ok)
                Logger.Info("Exiting with success code");
            else
                Logger.Error($"Exiting with failure code: {exitCode}");

            Console.ReadKey();
        }
    }
}
