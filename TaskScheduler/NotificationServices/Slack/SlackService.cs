using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SlackMessenger;
using TaskScheduler.Helper;

namespace TaskScheduler.NotificationServices.Slack
{
    public class SlackService : NotificationService
    {
        protected SlackClient Client { get; set; }
        public new NotificationType NotificationType { get; } = NotificationType.Slack;
        public string SenderName { get; set; }
        public string WebhookUrl { get; set; }
        public string IconUrl { get; set; }

        public SlackService() { }
        public SlackService(string sender, string webhookUrl, string icon, bool isDefaultService = false)
        {
            SenderName = sender;
            WebhookUrl = webhookUrl;
            IconUrl = icon;
            IsDefaultService = isDefaultService;
            Initial();
        }

        public sealed override void Initial()
        {
            if (Client == null)
                Client = new SlackClient(WebhookUrl);
        }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver))
                return SystemNotification.InvalidOperation;

            foreach (var id in receiver.SplitUp())
                if (!SendSingleSlackMessage(id, message, subject))
                    completed = false;

            return completed
                ? SystemNotification.SuccessfullyDone
                : SystemNotification.InternalError;
        }

        public override async Task<SystemNotification> SendAsync(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver)) return SystemNotification.InvalidOperation;
            var tasks = new List<Task>();

            foreach (var id in receiver.SplitUp())
                tasks.Add(new Task(() =>
                {
                    if (!SendSingleSlackMessage(id, message, subject))
                        completed = false;
                }));
            await Task.WhenAll(tasks.ToArray());

            return completed
                ? SystemNotification.SuccessfullyDone
                : SystemNotification.InternalError;
        }

        protected bool SendSingleSlackMessage(string receiver, string message, string subject)
        {
            try
            {
                subject = subject.Replace("**", "*");
                message = message.Replace("**", "*");
                Logger.Info($"Sending slack to #{receiver} channel ...");
                var msg = new Message($"{subject} \n\n {message}", receiver, SenderName, IconUrl);
                Client.Send(msg);
                Logger.Info($"Slack message sent to #{receiver} channel successful.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, $"Send telegram failed for telegram id: {receiver}");
                return false;
            }
        }
    }
}