using System;
using System.Linq;
using System.Threading.Tasks;
using TaskScheduler.Helper;

namespace TaskScheduler.NotificationServices.SMS
{
    public class SmsService : NotificationService
    {
        public new NotificationType NotificationType { get; } = NotificationType.Sms;
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderNumber { get; set; }

        public SmsService() { }

        public SmsService(string username, string password, string sender, bool isDefaultService = false)
        {
            Username = username;
            Password = password;
            SenderNumber = sender;
            IsDefaultService = isDefaultService;
            Initial();
        }

        public sealed override void Initial()
        {
            if (SenderNumber.Any(d => !(char.IsDigit(d) || d == '+')))
                throw new ArgumentException(SenderNumber, "The sender number must be positive numbers!");
        }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver)) return SystemNotification.InvalidOperation;
            var service = new RahyabSmsService(Username, Password, SenderNumber);
            foreach (var phone in receiver.Split(new[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                var cellphone = CellphoneNumber.Normalize(phone);

                try
                {
                    if (cellphone == null) throw new Exception($"Invalid Phone: {phone}");
                    service.SendSms(cellphone.PhoneNumber, subject.CleanText() + "\n\n" + message.CleanText());
                    Logger.Info($"SMS Sent successfully to: {cellphone.PhoneNumber}");
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, $"Send SMS failed for sending message to {phone}");
                    completed = false;
                }
            }

            return completed ? SystemNotification.SuccessfullyDone : SystemNotification.InternalError;
        }

        public override async Task<SystemNotification> SendAsync(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver)) return SystemNotification.InvalidOperation;
            var service = new RahyabSmsService(Username, Password, SenderNumber);
            foreach (var phone in receiver.Split(new[] { ",", ";", " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                var cellphone = CellphoneNumber.Normalize(phone);

                try
                {
                    if (cellphone == null) throw new Exception($"Invalid Phone: {phone}");
                    await service.SendSmsAsync(cellphone.PhoneNumber, subject.CleanText() + "\n\n" + message.CleanText());
                    Logger.Info($"SMS Sent successfully to: {cellphone.PhoneNumber}");
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, $"Send SMS failed for sending message to {phone}");
                    completed = false;
                }
            }

            return completed ? SystemNotification.SuccessfullyDone : SystemNotification.InternalError;
        }
    }
}