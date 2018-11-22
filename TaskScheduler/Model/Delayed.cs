using System;
using System.Globalization;
using Hangfire;
using NLog;
using TaskScheduler.Model.Enum;

namespace TaskScheduler.Model
{
    /// <summary>
    ///     Delayed jobs are executed only once too, but not immediately, after a certain time interval.
    /// </summary>
    public class Delayed : Job
    {
        public override string Register()
        {
            try
            {
                JobId = BackgroundJob.Schedule(() => Trigger(this), TriggerOn);

                Nlogger.Info($"The [{JobId}] added to hangfire delayed jobs successful.");

                return JobId;
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
                return exp.Message;
            }
        }

        /// <summary>
        ///     Continuations are executed when its parent job has been finished.
        /// </summary>
        /// <param name="action">The job, which should be continued after this job</param>
        public void ContinueWith(Action action)
        {
            BackgroundJob.ContinueWith(JobId, () => action());
        }

        #region Properties

        private static readonly Logger Nlogger = LogManager.GetCurrentClassLogger();

        public new JobType JobType
        {
            get => base.JobType;
            protected set => base.JobType = value;
        }

        public new TimeSpan TriggerOn
        {
            get => TimeSpan.ParseExact(base.TriggerOn, "G", CultureInfo.InvariantCulture);
            set => base.TriggerOn = value.ToString("G");
        }

        #endregion

        #region Constructors

        public Delayed()
        {
            JobType = JobType.Delayed;
        }

        public Delayed(IJob job) : base(job)
        {
            JobType = JobType.Delayed;
        }

        #endregion
    }
}