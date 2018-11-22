using System;
using System.Threading.Tasks;
using TaskScheduler.Helper;

namespace TaskScheduler.NotificationServices.SMS
{
    public class SmsService : NotificationService
    {
        public SmsService(string userName, string password, string sendNumber)
            : base(userName, password, sendNumber)
        {
        }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver)) return SystemNotification.InvalidOperation;
            var service = new RahyabSmsService(UserName, Password, Sender);
            foreach (var phone in receiver.Split(new[] {",", ";", " "}, StringSplitOptions.RemoveEmptyEntries))
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

        public override Task<SystemNotification> SendAsync(string receiver, string message, string subject)
        {
            throw new NotImplementedException();
        }
    }
}