using System.Threading.Tasks;

namespace TaskScheduler.NotificationServices
{
    public interface INotificationService
    {
        SystemNotification Send(string receiver, string message, string subject);

        Task<SystemNotification> SendAsync(string receiver, string message, string subject);
    }
}