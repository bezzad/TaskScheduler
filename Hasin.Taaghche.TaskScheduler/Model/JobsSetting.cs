using System.Collections.Generic;
using Hasin.Taaghche.TaskScheduler.NotificationServices;
using Newtonsoft.Json;

namespace Hasin.Taaghche.TaskScheduler.Model
{
    public class JobsSetting
    {
        [JsonProperty(PropertyName = "jobs", NullValueHandling = NullValueHandling.Include, Required = Required.AllowNull)]
        public IList<object> Jobs { get; set; }

        [JsonProperty(PropertyName = "notifications", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Notification> Notifications { get; set; } // Notification on result events
    }
}