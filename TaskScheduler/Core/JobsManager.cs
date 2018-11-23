using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hangfire;
using Hangfire.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using TaskScheduler.Helper;
using TaskScheduler.Model;
using TaskScheduler.NotificationServices;

namespace TaskScheduler.Core
{
    public static class JobsManager
    {
        #region Properties

        private static string JsonSetting { get; set; } = "";
        private static readonly Logger Nlogger = LogManager.GetCurrentClassLogger();
        public static SettingWrapper Setting { get; set; }

        #endregion


        #region Constructors

        /// <summary>
        ///     Check setting file and reset jobs if changed.
        /// </summary>
        /// <param name="settingPath">setting file path.</param>
        /// <param name="nextCheckupInterval">next checkup time by minute interval</param>
        public static void CheckupSetting(string settingPath, int nextCheckupInterval)
        {
            var settingChanged = false;
            try
            {
                var setting = FileManager.ReadFileSafely(settingPath);

                if (string.IsNullOrEmpty(setting))
                {
                    Nlogger.Warn("Setting file not found!");
                    CreateSettingFile(settingPath);
                    CheckupSetting(settingPath, nextCheckupInterval);
                    return;
                }

                if (setting.Equals(JsonSetting, StringComparison.OrdinalIgnoreCase))
                {
                    Nlogger.Info("Setting was not changed.");
                    return;
                }
                Nlogger.Info("Setting was changed, reset jobs...");
                settingChanged = true;

                // clear old jobs
                ClearAllRecurringJobs();
                PurgeJobs();
                //
                // Add new jobs from setting file
                JsonSetting = setting;
                Setting = DeserializeFromJson(JsonSetting);

                RegisterJobs();

                Nlogger.Info("Jobs are ready to work.");
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
            }
            finally
            {
                if (settingChanged)
                    RecurringJob.AddOrUpdate(() => CheckupSetting(settingPath, nextCheckupInterval),
                        Cron.MinuteInterval(nextCheckupInterval), TimeZoneInfo.Local);
            }
        }

        #endregion



        #region Methods

        public static void RegisterJobs()
        {
            if (Setting?.Jobs?.Any() != true) return;

            foreach (var job in Setting.Jobs) job?.Register();
        }

        public static IJob AddJob(IJob job)
        {
            Setting.Jobs.Add(job);
            job.Register();
            return job;
        }

        public static string SerializeToJson()
        {
            return Setting == null ? "" : JsonConvert.SerializeObject(Setting, Formatting.Indented);
        }

        public static SettingWrapper DeserializeFromJson(string json)
        {
            var result = new SettingWrapper();

            try
            {
                var asm = Assembly.GetAssembly(typeof(IJob));
                json = json.Replace("{#version}", asm.GetName().Version.ToString(3));
                var setting = JsonConvert.DeserializeObject<JobsSetting>(json);

                result.Jobs = new List<IJob>();
                result.NotificationServices = new List<INotificationService>();
                result.Notifications = setting.Notifications;

                if (setting.Jobs == null || setting.NotificationServices == null) return null;
                //
                // parse notification services to real type
                foreach (JObject ns in setting.NotificationServices)
                {
                    try
                    {
                        var serviceType = ns.GetValue("notificationType")?.ToString();
                        if (string.IsNullOrWhiteSpace(serviceType)) continue;

                        var service = asm.GetTypes()
                            .FirstOrDefault(t => t.Name.Equals(serviceType, StringComparison.OrdinalIgnoreCase));
                        var serviceObj = (INotificationService)ns.ToObject(service);
                        serviceObj?.Initial();
                        result.NotificationServices.Add(serviceObj);
                    }
                    catch (Exception e)
                    {
                        Nlogger.Error(e);
                    }
                }
                //
                // parse jobs to real type
                foreach (JObject j in setting.Jobs)
                {
                    try
                    {
                        var jobTypeName = j.GetValue("jobType")?.ToString();

                        if (string.IsNullOrWhiteSpace(jobTypeName)) continue;

                        var jobType = asm.GetTypes()
                                .FirstOrDefault(t => t.Name.Equals(jobTypeName, StringComparison.OrdinalIgnoreCase));
                        var jobObj = (IJob)j.ToObject(jobType);
                        //
                        // if the job hasn't any notification and exist at last one public notification 
                        // then insert public notifications
                        if (jobObj != null && jobObj.Notifications?.Any() != true &&
                            setting.Notifications?.Any() == true) jobObj.Notifications = setting.Notifications;
                        result.Jobs.Add(jobObj);
                    }
                    catch (Exception e)
                    {
                        Nlogger.Error(e);
                    }
                }
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
            }

            return result;
        }

        public static void ClearAllRecurringJobs()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                    RecurringJob.RemoveIfExists(recurringJob.Id);
            }
        }

        public static void PurgeJobs()
        {
            var monitor = JobStorage.Current?.GetMonitoringApi();
            var toDelete = new List<string>();

            if (monitor == null) return;

            foreach (var queue in monitor.Queues())
                for (var i = 0; i < Math.Ceiling(queue.Length / 1000d); i++)
                    monitor.EnqueuedJobs(queue.Name, 1000 * i, 1000)
                        .ForEach(x => toDelete.Add(x.Key));

            foreach (var jobId in toDelete) BackgroundJob.Delete(jobId);
        }

        public static void CreateSettingFile(string settingPath)
        {
            Nlogger.Info("Trying to create setting file...");
            FileManager.WriteFileSafely(settingPath, JsonConvert.SerializeObject(new JobsSetting()));
            Nlogger.Info($"Setting file successfully created at:\t{settingPath}");
        }

        #endregion
    }
}