using System.Collections.Generic;
using Hasin.Taaghche.TaskScheduler.Model.Enum;
using Hasin.Taaghche.TaskScheduler.NotificationServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hasin.Taaghche.TaskScheduler.Model
{
    public interface IJob
    {
        [JsonIgnore]
        string JobId { get; set; }

        [JsonProperty(PropertyName = "name", NullValueHandling = NullValueHandling.Ignore)]
        string Name { get; set; }

        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        string Description { get; set; }

        [JsonProperty(PropertyName = "enable", NullValueHandling = NullValueHandling.Ignore)]
        bool Enable { get; set; }

        [JsonRequired]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "jobType", NullValueHandling = NullValueHandling.Include, Required = Required.Always)]
        JobType JobType { get; set; }

        [JsonRequired]
        [JsonProperty(PropertyName = "actionName", NullValueHandling = NullValueHandling.Include, Required = Required.Always)]
        string ActionName { get; set; }

        [JsonProperty(PropertyName = "actionParameters", NullValueHandling = NullValueHandling.Include)]
        IDictionary<string, object> ActionParameters { get; set; }
        
        [JsonProperty(PropertyName = "triggerOn", NullValueHandling = NullValueHandling.Include, Required = Required.AllowNull)]
        string TriggerOn { get; set; } // Cron expression or TimeSpan

        [JsonProperty(PropertyName = "notifications", NullValueHandling = NullValueHandling.Ignore)]
        IList<Notification> Notifications { get; set; } // Notification on result events

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "notifyCondition", NullValueHandling = NullValueHandling.Include)]
        NotifyCondition NotifyCondition { get; set; }

        [JsonProperty(PropertyName = "notifyConditionResult", NullValueHandling = NullValueHandling.Include)]
        string NotifyConditionResult { get; set; }

        /// <summary>
        /// Add or update a job in hangfire to do lists.
        /// </summary>
        /// <returns>Get added job id</returns>
        string Register();

        /// <summary>
        /// Run job action now.
        /// </summary>
        void Trigger(Job job);
        
    }
}