using System.Collections.Generic;
using Newtonsoft.Json;
using TaskScheduler.NotificationServices;

namespace TaskScheduler.Jobs
{
    public class JobsSetting
    {
        [JsonProperty(PropertyName = "jobs", NullValueHandling = NullValueHandling.Include,
            Required = Required.AllowNull)]
        public IList<object> Jobs { get; set; }

        [JsonProperty(PropertyName = "notifications", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Notification> Notifications { get; set; } // Notification on result events

        [JsonProperty(PropertyName = "notificationServices", NullValueHandling = NullValueHandling.Ignore)]
        public IList<object> NotificationServices { get; set; } // Notification services data
    }
}