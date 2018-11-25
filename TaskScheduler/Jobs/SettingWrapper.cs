using System.Collections.Generic;
using Newtonsoft.Json;
using TaskScheduler.NotificationServices;

namespace TaskScheduler.Jobs
{
    public class SettingWrapper
    {
        [JsonProperty(PropertyName = "jobs", NullValueHandling = NullValueHandling.Include,
            Required = Required.AllowNull)]
        public IList<IJob> Jobs { get; set; }

        [JsonProperty(PropertyName = "notifications", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Notification> Notifications { get; set; } // Notification on result events

        [JsonProperty(PropertyName = "notificationServices", NullValueHandling = NullValueHandling.Ignore)]
        public IList<INotificationService> NotificationServices { get; set; } // Notification services data
    }
}