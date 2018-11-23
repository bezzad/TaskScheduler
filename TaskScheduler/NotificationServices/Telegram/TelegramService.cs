using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskScheduler.Helper;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

namespace TaskScheduler.NotificationServices.Telegram
{
    public class TelegramService : NotificationService
    {
        public new NotificationType NotificationType { get; } = NotificationType.Telegram;
        public TelegramBotClient Bot { get; set; }
        public string Username { get; set; }
        public string ApiKey { get; set; }
        public string SenderBot { get; set; }

        public TelegramService() { }
        public TelegramService(string username, string apiKey, string senderBot, bool isDefaultService = false)
        {
            Username = username;
            ApiKey = apiKey;
            SenderBot = senderBot;
            IsDefaultService = isDefaultService;
        }

        public async Task InitialAsync()
        {
            if (Bot == null)
            {
                Logger.Info("Running telegram bot...");
                Bot = new TelegramBotClient(ApiKey);
                Bot.StartReceiving();
                Bot.OnMessage += Bot_OnMessage;

                var me = await Bot.GetMeAsync();
                Logger.Info($"The {me.Username} bot is running.");
            }
        }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver))
                return SystemNotification.InvalidOperation;

            Parallel.ForEach(receiver.SplitUp(), id =>
            {
                try
                {
                    Bot.SendTextMessageAsync(id.Trim(), $"{subject} \n {message}", parseMode: ParseMode.Markdown)
                        .ContinueWith(result => Logger.Info($"Telegram message sent to {id} id successful."));
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, $"Send telegram failed for telegram id: {id}");
                    completed = false;
                }
            });

            return completed
                ? SystemNotification.SuccessfullyDone
                : SystemNotification.InternalError;
        }

        public override async Task<SystemNotification> SendAsync(string receiver, string message, string subject)
        {
            if (string.IsNullOrEmpty(receiver)) return SystemNotification.InvalidOperation;
            var completed = true;
            var tasks = new List<Task>();
            var ids = receiver.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var id in ids)
            {
                try
                {
                    Logger.Info($"Sending telegram to id: {id} ...");
                    tasks.Add(Bot.SendTextMessageAsync(id.Trim(), $"{subject} \n {message}",
                        parseMode: ParseMode.Markdown));
                    Logger.Info($"Telegram message sent to {id} id successful.");
                }
                catch (Exception ex)
                {
                    completed = false;
                    Logger.Fatal(ex, $"Send telegram failed for telegram id: {id}");
                }
            }

            await Task.WhenAll(tasks.ToArray());

            return completed ? SystemNotification.SuccessfullyDone : SystemNotification.InternalError;
        }

        protected void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Logger.Info($"The {e.Message.From.Username} user call notification bot by message: {e.Message.Text}");
            Bot.SendTextMessageAsync(e.Message.From.Id, e.Message.From.Id.ToString());
            Logger.Info($"Response to user by user id: {e.Message.From.Id}");
        }
    }
}