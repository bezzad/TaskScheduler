using System.Threading.Tasks;
using NLog;

namespace TaskScheduler.NotificationServices
{
    public abstract class NotificationService : INotificationService
    {
        protected ILogger Logger { get; set; }
        public NotificationType NotificationType { get; set; }
        public string ServiceName { get; set; }
        public bool IsDefaultService { get; set; }

        protected NotificationService()
        {
            Logger = LogManager.GetCurrentClassLogger();
        }


        public abstract SystemNotification Send(string receiver, string message, string subject);
        public abstract Task<SystemNotification> SendAsync(string receiver, string message, string subject);
        public virtual void Initial() { }
    }
}