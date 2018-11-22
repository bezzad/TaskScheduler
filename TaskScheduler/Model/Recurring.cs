using System;
using Hangfire;
using NLog;
using TaskScheduler.Model.Enum;

namespace TaskScheduler.Model
{
    /// <summary>
    ///     Recurring jobs fire many times on the specified CRON schedule. as <see cref="Hangfire.Cron" />
    /// </summary>
    public class Recurring : Job
    {
        public override string Register()
        {
            try
            {
                JobId = $"{Name ?? ActionName}: {DateTime.Now.GetHashCode()}";

                RecurringJob.AddOrUpdate(JobId, () => Trigger(this), TriggerOn, TimeZoneInfo.Local);

                Nlogger.Info($"The [{JobId}] added to hangfire recurring jobs successful.");

                return JobId;
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
                return exp.Message;
            }
        }

        #region Properties

        private static readonly Logger Nlogger = LogManager.GetCurrentClassLogger();

        public new JobType JobType
        {
            get => base.JobType;
            protected set => base.JobType = value;
        }

        #endregion

        #region Constructors

        public Recurring()
        {
            JobType = JobType.Recurring;
        }

        public Recurring(IJob job) : base(job)
        {
            JobType = JobType.Recurring;
        }

        #endregion
    }
}