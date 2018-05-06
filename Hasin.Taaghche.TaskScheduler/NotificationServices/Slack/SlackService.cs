using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SlackMessenger;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices.Slack
{
    public class SlackService : NotificationService
    {
        public static SlackClient Client { get; set; }

        public SlackService(string userName, string webhookUrl, string icon)
            : base(userName, webhookUrl, icon)
        {
            Client = new SlackClient(webhookUrl);
        }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver))
                return SystemNotification.InvalidOperation;

            foreach (var id in receiver.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    Logger.Info($"Sending slack to #{id} channel ...");
                    var msg = new Message(text: $"{subject} \n\n {message}", channel: id, username: UserName, icon: Sender);
                    Client.Send(msg);
                    Logger.Info($"Slack message sent to #{id} channel successful.");
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, $"Send slack message failed for channel: #{id}");
                    completed = false;
                }
            }

            return completed
                ? SystemNotification.SuccessfullyDone
                : SystemNotification.InternalError;
        }

        public override async Task<SystemNotification> SendAsync(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver)) return SystemNotification.InvalidOperation;
            var tasks = new List<Task>();
            var ids = receiver.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var id in ids)
            {
                tasks.Add(new Task(() =>
                {
                    try
                    {
                        Logger.Info($"Sending slack to #{id} channel ...");

                        var msg = new Message(text: $"{subject} \n\n {message}", channel: id, username: UserName, icon: Sender);
                        Client.Send(msg);

                        Logger.Info($"Slack message sent to #{id} channel successful.");
                    }
                    catch (Exception ex)
                    {
                        completed = false;
                        Logger.Fatal(ex, $"Send telegram failed for telegram id: {id}");
                    }
                }));
            }
            await Task.WhenAll(tasks.ToArray());

            return completed
                ? SystemNotification.SuccessfullyDone
                : SystemNotification.InternalError;
        }
    }
}
