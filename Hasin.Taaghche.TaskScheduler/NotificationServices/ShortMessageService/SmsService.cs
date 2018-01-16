using System;
using System.Threading.Tasks;
using Hasin.Taaghche.TaskScheduler.Helper;
using Hasin.Taaghche.TaskScheduler.Service_References.GamamnSmsService;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices.ShortMessageService
{
    public class SmsService : NotificationService
    {
        public SmsService(string userName, string password, string sendNumber)
            : base(userName, password, sendNumber) { }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver)) return SystemNotification.InvalidOperation;
            RahyabSmsService service = new RahyabSmsService(UserName, Password, Sender);
            foreach (var phone in receiver.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries))
            {
                var cellphone = CellphoneNumber.Normalize(phone);

                try
                {
                    if (cellphone == null) throw new Exception($"Invalid Phone: {phone}");

                    service.SendSms(cellphone.PhoneNumber, message);
                    Logger.Info($"SMS Sent successfully to: {cellphone.PhoneNumber}");
                    
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, $"Send sms failed for sending message to {phone}");
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