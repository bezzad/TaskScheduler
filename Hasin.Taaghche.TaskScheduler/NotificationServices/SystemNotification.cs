using Newtonsoft.Json;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class SystemNotification
    {

        public static readonly SystemNotification SuccessfullyDone = new SystemNotification
        {
            Status = Status.Successful,
            Message = null,
            AdditionalData = "عملیات با موفقیت انجام شد"
        };

        public static readonly SystemNotification InternalError = new SystemNotification
        {
            Status = Status.BadContract,
            Message = "خطایی در سیستم رخ داده است. لطفا دوباره تلاش کنید.",
            AdditionalData = null
        };

        public static readonly SystemNotification InvalidOperation = new SystemNotification
        {
            Status = Status.BadContract,
            Message = "درخواست شما قابل اجرا نیست",
            AdditionalData = null
        };

        public static readonly SystemNotification TooMuchRecipients = new SystemNotification
        {
            Status = Status.BadContract,
            Message = "نمی‌توانید ایمیل را به بیش از ۹۹۹ نفر ارسال کنید",
            AdditionalData = null
        };

        public static readonly SystemNotification SendingLimitExceeded = new SystemNotification
        {
            Status = Status.BadContract,
            Message = "لطفا بعدا دوباره تلاش کنید",
            AdditionalData = null
        };


        [JsonProperty("additionalData")]
        public string AdditionalData { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("status")]
        public Status Status { get; set; }

    }
}
