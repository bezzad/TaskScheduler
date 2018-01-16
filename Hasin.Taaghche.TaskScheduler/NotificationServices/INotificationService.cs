using System.Threading.Tasks;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices
{
    public interface INotificationService
    {
        SystemNotification Send(string receiver, string message, string subject);

        Task<SystemNotification> SendAsync(string receiver, string message, string subject);
    }
}