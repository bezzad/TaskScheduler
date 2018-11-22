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
                Jobs = DeserializeFromJson(JsonSetting);

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

        #region Properties

        private static string JsonSetting { get; set; } = "";
        private static readonly Logger Nlogger = LogManager.GetCurrentClassLogger();
        public static List<IJob> Jobs { get; set; } = new List<IJob>();

        #endregion

        #region Methods

        public static void RegisterJobs()
        {
            if (Jobs?.Any() != true) return;

            foreach (var job in Jobs) job?.Register();
        }

        public static IJob AddJob(IJob job)
        {
            Jobs.Add(job);
            job.Register();
            return job;
        }

        public static string SerializeToJson()
        {
            var result = new JobsSetting
            {
                Jobs = Jobs.Select(j => (object) j).ToList(),
                Notifications = new List<Notification>()
            };

            //
            // Find shared notifications between all jobs
            if (result.Jobs?.Any() == true)
            {
                var notifications = ((IJob) result.Jobs.FirstOrDefault())?.Notifications;
                if (notifications != null)
                {
                    foreach (var notify in notifications)
                        if (result.Jobs.Skip(1).All(x => ((IJob) x).Notifications.Any(n => n == notify)))
                            result.Notifications.Add(notify);

                    //
                    // Remove shared notifications from jobs
                    foreach (var notify in result.Notifications)
                    {
                        var all = result.Jobs.All(j => ((IJob) j).Notifications.Remove(notify));
                    }
                }
            }

            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        public static List<IJob> DeserializeFromJson(string json)
        {
            var result = new List<IJob>();

            try
            {
                var jobAsm = Assembly.GetAssembly(typeof(IJob));

                var setting = JsonConvert.DeserializeObject<JobsSetting>(json);

                if (setting?.Jobs == null) return null;

                foreach (JObject j in setting.Jobs)
                    try
                    {
                        var jobTypeName = j.GetValue("jobType")?.ToString();

                        if (string.IsNullOrEmpty(jobTypeName)) continue;

                        var jobType =
                            jobAsm.GetTypes()
                                .FirstOrDefault(t => t.Name.Equals(jobTypeName, StringComparison.OrdinalIgnoreCase));
                        var jobObj = (IJob) j.ToObject(jobType);
                        //
                        // if the job hasn't any notification and exist at last one public notification 
                        // then insert public notifications
                        if (jobObj != null && jobObj.Notifications?.Any() != true &&
                            setting.Notifications?.Any() == true) jobObj.Notifications = setting.Notifications;
                        result.Add(jobObj);
                    }
                    catch (Exception e)
                    {
                        Nlogger.Error(e);
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