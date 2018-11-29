using Newtonsoft.Json;

namespace TaskScheduler.NotificationServices
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SystemNotification
    {
        public static readonly SystemNotification SuccessfullyDone = new SystemNotification
        {
            Status = Status.Successful,
            Message = null,
            AdditionalData = "The operation successful completed."
        };

        public static readonly SystemNotification InternalError = new SystemNotification
        {
            Status = Status.BadContract,
            Message = "One error occurred in system, Please try again.",
            AdditionalData = null
        };

        public static readonly SystemNotification InvalidOperation = new SystemNotification
        {
            Status = Status.BadContract,
            Message = "Your request is invalid!",
            AdditionalData = null
        };

        public static readonly SystemNotification TooMuchRecipients = new SystemNotification
        {
            Status = Status.BadContract,
            Message = "You can't send email to more than 999 people.",
            AdditionalData = null
        };

        public static readonly SystemNotification SendingLimitExceeded = new SystemNotification
        {
            Status = Status.BadContract,
            Message = "Please try again later.",
            AdditionalData = null
        };


        [JsonProperty("additionalData")] public string AdditionalData { get; set; }

        [JsonProperty("message")] public string Message { get; set; }

        [JsonProperty("status")] public Status Status { get; set; }
    }
}