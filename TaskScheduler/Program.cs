using System;
using System.Diagnostics;
using System.Threading;
using NLog;
using Topshelf;
using AssemblyInfo = TaskScheduler.Helper.AssemblyInfo;

namespace TaskScheduler
{
    public class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        // ReSharper disable once UnusedParameter.Local
        private static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;

            var rc = HostFactory.Run(configurator =>
            {
                configurator.UseNLog(Logger.Factory);
                configurator.RunAsLocalSystem();
                configurator.SetDisplayName(
                    $"{AssemblyInfo.Company} {AssemblyInfo.Title} v{AssemblyInfo.Version.ToString(3)}");
                configurator.SetDescription(AssemblyInfo.Description);
                configurator.SetServiceName(AssemblyInfo.Product);
                configurator.SetStartTimeout(TimeSpan.FromMinutes(5));
                configurator.OnException(exception =>
                {
                    LogManager.GetCurrentClassLogger().Fatal(exception);
                    Debugger.Break();
                });
                configurator.Service<Service>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(settings => new Service(settings, Properties.Settings.Default.Port));
                    serviceConfigurator.WhenStarted(service => service.Start());
                    serviceConfigurator.WhenStopped(service => service.Stop());
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

                configurator.StartAutomatically(); // only executed if the service is being installed.
            });
            if (rc == TopshelfExitCode.Ok)
                Logger.Info("Exiting with success code");
            else
                Logger.Error($"Exiting with failure code: {rc}");

            var exitCode = Convert.ChangeType(rc, rc.GetTypeCode()) as int?;
            Environment.ExitCode = exitCode ?? 1;

            Console.ReadKey();
        }
    }
}