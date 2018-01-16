using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Hasin.Taaghche.TaskScheduler.Helper;
using System.Collections.Generic;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices
{
    public class Notification : IEquatable<Notification>, IEqualityComparer<Notification>, ICloneable
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "notifyType", NullValueHandling = NullValueHandling.Include)]
        public NotificationType NotifyType { get; set; }

        [JsonProperty(PropertyName = "receiver", NullValueHandling = NullValueHandling.Include)]
        public string Receiver { get; set; }



        

        public void Notifying(string message, string subject)
        {
            NotifyType.Factory().Send(Receiver, message, subject);
        }



        public bool Equals(Notification other)
        {
            return NotifyType == other?.NotifyType &&
                   Receiver == other.Receiver;
        }

        public bool Equals(Notification x, Notification y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Notification obj)
        {
            return obj.GetHashCode();
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
            return ReferenceEquals(notifyA, null)
                ? ReferenceEquals(notifyB, null)
                : notifyA.Equals(notifyB);
        }

        public static bool operator !=(Notification notifyA, Notification notifyB)
        {
            return !(notifyA == notifyB);
        }


        public object Clone()
        {
            return new Notification()
            {
                NotifyType = NotifyType,
                Receiver = Receiver
            };
        }
    }
}