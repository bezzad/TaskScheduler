using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using NLog;
using TaskScheduler.Core;
using TaskScheduler.Model.Enum;
using TaskScheduler.NotificationServices;

namespace TaskScheduler.Model
{
    public class Job : IJob
    {
        private static Logger Nlogger { get; } = LogManager.GetCurrentClassLogger();

        #region IJob Implementation

        public string JobId { get; set; }
        public bool Enable { get; set; } = true;
        public string Name { get; set; }
        public string Description { get; set; }
        public JobType JobType { get; set; }
        public string ActionName { get; set; }
        public IDictionary<string, object> ActionParameters { get; set; }
        public string TriggerOn { get; set; }
        public IList<Notification> Notifications { get; set; }
        public NotifyCondition NotifyCondition { get; set; }
        public string NotifyConditionResult { get; set; }


        public virtual string Register()
        {
            throw new ActionNotSupportedException(
                "This class is abstract for register job! Use of child classes instead this.");
        }


        public bool CompareByNotifyCondition(string actionResult)
        {
            switch (NotifyCondition)
            {
                case NotifyCondition.Equals:
                    return actionResult.Equals(NotifyConditionResult, StringComparison.OrdinalIgnoreCase);
                case NotifyCondition.NotEquals:
                    return !actionResult.Equals(NotifyConditionResult, StringComparison.OrdinalIgnoreCase);
                case NotifyCondition.MoreThan:
                    return String.Compare(actionResult, NotifyConditionResult, StringComparison.OrdinalIgnoreCase) > 0;
                case NotifyCondition.EqualsOrMoreThan:
                    return String.Compare(actionResult, NotifyConditionResult, StringComparison.OrdinalIgnoreCase) >= 0;
                case NotifyCondition.LessThan:
                    return String.Compare(actionResult, NotifyConditionResult, StringComparison.OrdinalIgnoreCase) < 0;
                case NotifyCondition.EqualsOrLessThan:
                    return String.Compare(actionResult, NotifyConditionResult, StringComparison.OrdinalIgnoreCase) <= 0;
                default:
                    return false;
            }
        }

        public void Trigger(Job job)
        {
            MapToThis(job);
            if (!Enable) return;

            var result = job.Run();
            OnTriggerNotification(result);
        }

        public void OnTriggerNotification(string result)
        {
            if (Notifications == null || !Notifications.Any() || NotifyCondition == NotifyCondition.None) return;

            var subject = $"> {(Debugger.IsAttached ? "`DEBUG MODE`" : "")} *{Name}*";
            var body = $"\n`Result`: ```{result}``` \n\n" +
                       $"```Action: {ActionName}\n";

            if (ActionParameters?.Any() == true)
            {
                body += "Arguments: \n";
                foreach (var arg in ActionParameters.Where((k, v) => !String.IsNullOrEmpty(v.ToString())))
                {
                    var val = String.Concat(arg.Value.ToString()
                        .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => $"{x}\n\t"));
                    val = val.Remove(val.LastIndexOf('\t'));
                    body += $"  • {arg.Key}: {val}";
                }

                body = body.Replace("\"", "") + "```";
            }

            foreach (var notify in Notifications.Where(n => CompareByNotifyCondition(result)))
                try
                {
#if !TestWithoutNotify
                    notify.Notifying(body, subject);
#endif
                }
                catch (Exception exp)
                {
                    Nlogger.Error(exp);
                }
        }

        public void MapToThis(IJob job)
        {
            if (job == null) return;

            Name = job.Name;
            Description = job.Description;
            Enable = job.Enable;
            JobType = job.JobType;
            ActionName = job.ActionName;
            ActionParameters = job.ActionParameters;
            Notifications = job.Notifications;
            JobId = job.JobId;
            TriggerOn = job.TriggerOn;
            NotifyCondition = job.NotifyCondition;
            NotifyConditionResult = job.NotifyConditionResult;
        }

        #endregion

        #region Constructors

        public Job()
        {
        }

        public Job(IJob job)
        {
            MapToThis(job);
        }

        #endregion
    }
}