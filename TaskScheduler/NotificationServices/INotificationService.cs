using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TaskScheduler.NotificationServices
{
    public interface INotificationService
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "notificationType", NullValueHandling = NullValueHandling.Include)]
        NotificationType NotificationType { get; set; }

        [JsonProperty(PropertyName = "serviceName", NullValueHandling = NullValueHandling.Include)]
        string ServiceName { get; set; }

        // is default service for the special notification type's
        [JsonProperty(PropertyName = "isDefaultService", NullValueHandling = NullValueHandling.Ignore)]
        bool IsDefaultService { get; set; } 

        SystemNotification Send(string receiver, string message, string subject);

        Task<SystemNotification> SendAsync(string receiver, string message, string subject);
        void Initial();
    }
}