using System.Threading.Tasks;
using NLog;

namespace TaskScheduler.NotificationServices
{
    public abstract class NotificationService : INotificationService
    {
        protected NotificationService(string userName, string password, string sender)
        {
            Logger = LogManager.GetCurrentClassLogger();
            Password = password;
            UserName = userName;
            Sender = sender;
        }

        protected ILogger Logger { get; set; }
        protected string Password { get; set; }
        protected string Sender { get; set; }
        protected string UserName { get; set; }

        public abstract SystemNotification Send(string receiver, string message, string subject);
        public abstract Task<SystemNotification> SendAsync(string receiver, string message, string subject);
    }
}