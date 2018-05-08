using System;
using System.Collections.Generic;
using Hasin.Taaghche.TaskScheduler.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices
{
    public class Notification : IEquatable<Notification>, IEqualityComparer<Notification>, ICloneable
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "notifyType", NullValueHandling = NullValueHandling.Include)]
        public NotificationType NotifyType { get; set; }

        [JsonProperty(PropertyName = "receiver", NullValueHandling = NullValueHandling.Include)]
        public string Receiver { get; set; }

        public object Clone()
        {
            return new Notification
            {
                NotifyType = NotifyType,
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
            return NotifyType == other?.NotifyType &&
                   Receiver == other.Receiver;
        }


        public void Notifying(string message, string subject)
        {
            NotifyType.Factory().Send(Receiver, message, subject);
        }

        public override int GetHashCode()
        {
            return NotifyType.GetHashCode() ^
                   Receiver.GetHashCode();
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