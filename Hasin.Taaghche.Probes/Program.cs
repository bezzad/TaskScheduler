using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
using Topshelf;

namespace Hasin.Taaghche.Probes
{
    class Program
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var exitCode = HostFactory.Run(configurator =>
            {
                var version = Assembly.GetEntryAssembly().GetName().Version.ToString(3);
                configurator.UseNLog(Logger.Factory);
                configurator.RunAsLocalSystem();
                configurator.SetDisplayName($"Taaghche Probes v{version}");
                configurator.SetDescription("Web API service for monitoring and probs other APIs and services");
                configurator.SetServiceName("TaaghcheProbes");
                configurator.SetStartTimeout(TimeSpan.FromMinutes(5));
                configurator.OnException(exception =>
                {
                    LogManager.GetCurrentClassLogger().Fatal(exception);
                    Debugger.Break();
                });
                configurator.Service<Service>(serviceConfigurator =>
                {
                    serviceConfigurator.ConstructUsing(settings => new Service(settings, 32032));
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
