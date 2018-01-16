using System;
using Hangfire;
using Hasin.Taaghche.TaskScheduler.Model.Enum;
using NLog;

namespace Hasin.Taaghche.TaskScheduler.Model
{
    /// <summary>
    /// Fire-and-forget jobs are executed only once and almost immediately after creation.
    /// </summary>
    public class FireAndForget : Job
    {
        #region Properties

        private static readonly Logger Nlogger = LogManager.GetCurrentClassLogger();

        public new JobType JobType
        {
            get { return JobType.FireAndForget; }
            protected set { base.JobType = value;}
        }

        public new string TriggerOn
        {
            get { return null; }
            protected set { base.TriggerOn = value; }
        }

        #endregion

        #region Constructors

        public FireAndForget() { JobType = JobType.FireAndForget; }

        public FireAndForget(IJob job) : base(job) { JobType = JobType.FireAndForget; }

        #endregion


        public override string Register()
        {
            try
            {
                JobId = BackgroundJob.Enqueue(() => Trigger(this));

                Nlogger.Info($"The [{JobId}] added to hangfire fire and forget jobs successful.");

                return JobId;
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
                return exp.Message;
            }
        }

        /// <summary>
        /// Continuations are executed when its parent job has been finished.
        /// </summary>
        /// <param name="action">The job, which should be continued after this job</param>
        public void ContinueWith(Action action)
        {
            BackgroundJob.ContinueWith(JobId, () => action());
        }
    }
}
