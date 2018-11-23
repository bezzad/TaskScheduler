using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TaskScheduler.Core;

namespace TaskScheduler.NotificationServices
{
    public class Notification : IEquatable<Notification>, IEqualityComparer<Notification>, ICloneable
    {
        [JsonProperty(PropertyName = "notificationServiceName", NullValueHandling = NullValueHandling.Include)]
        public string NotificationServiceName { get; set; }

        [JsonProperty(PropertyName = "receiver", NullValueHandling = NullValueHandling.Include)]
        public string Receiver { get; set; }

        public object Clone()
        {
            return new Notification
            {
                NotificationServiceName = NotificationServiceName,
                Receiver = Receiver
            };
        }

        public bool Equals(Notification x, Notification y)
        {
            return x?.Equals(y) == true;
        }

        public int GetHashCode(Notification obj)
        {
            return obj.GetHashCode();
        }


        public bool Equals(Notification other)
        {
            return NotificationServiceName == other?.NotificationServiceName &&
                   Receiver == other?.Receiver;
        }


        public void Notify(string message, string subject)
        {
            var service =
                JobsManager.Setting.NotificationServices.FirstOrDefault(ns =>
                    ns.ServiceName.Equals(NotificationServiceName, StringComparison.OrdinalIgnoreCase));

            service?.Send(Receiver, message, subject);
        }

        public async Task NotifyAsync(string message, string subject)
        {
            var service = GetNotificationService();
            if (service != null)
                await service.SendAsync(Receiver, message, subject);
        }

        public INotificationService GetNotificationService()
        {
            return 
                JobsManager.Setting.NotificationServices.FirstOrDefault(ns =>
                    ns.ServiceName.Equals(NotificationServiceName, StringComparison.OrdinalIgnoreCase));
        }


        public override int GetHashCode()
        {
            if (string.IsNullOrWhiteSpace(NotificationServiceName) || string.IsNullOrWhiteSpace(Receiver))
                return 0;

            return NotificationServiceName.GetHashCode() ^ Receiver.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var notifyObj = obj as Notification;

            return notifyObj != null && Equals(notifyObj);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static bool operator ==(Notification notifyA, Notification notifyB)
        {
            return notifyA?.Equals(notifyB) ?? ReferenceEquals(notifyB, null);
        }

        public static bool operator !=(Notification notifyA, Notification notifyB)
        {
            return !(notifyA == notifyB);
        }
    }
}