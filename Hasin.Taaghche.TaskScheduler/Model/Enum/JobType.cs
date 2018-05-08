namespace Hasin.Taaghche.TaskScheduler.Model.Enum
{
    /// <summary>
    ///     Hangfire Job Types
    /// </summary>
    public enum JobType
    {
        /// <summary>
        ///     Fire-and-forget jobs are executed only once and almost immediately after creation.
        ///     Method: <see cref="Hangfire.BackgroundJob.Enqueue" />
        /// </summary>
        FireAndForget = 1,

        /// <summary>
        ///     Delayed jobs are executed only once too, but not immediately, after a certain time interval.
        ///     Method: <see cref="Hangfire.BackgroundJob.Schedule" />
        /// </summary>
        Delayed = 2,

        /// <summary>
        ///     Recurring jobs fire many times on the specified <see cref="Hangfire.Cron" /> schedule.
        ///     Method: <see cref="Hangfire.RecurringJob.AddOrUpdate" />
        /// </summary>
        Recurring = 3
    }
}