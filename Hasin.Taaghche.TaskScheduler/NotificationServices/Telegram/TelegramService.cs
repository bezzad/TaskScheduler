﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hasin.Taaghche.TaskScheduler.Helper;
using Telegram.Bot;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices.Telegram
{
    public class TelegramService : NotificationService
    {
        public static TelegramBotClient Bot { get; set; }

        public TelegramService(string userName, string apiKey, string senderBot)
            : base(userName, apiKey, senderBot)
        {
            if (Bot != null) return;

            Logger.Info("Running telegram bot...");
            Bot = new TelegramBotClient(Password);
            Bot.StartReceiving();
            Bot.OnMessage += Bot_OnMessage;
            Task.Run(async () =>
            {
                var me = await Bot.GetMeAsync();
                Logger.Info($"The {me.Username} bot is running.");
            });
        }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver))
                return SystemNotification.InvalidOperation;
            
            foreach (var id in receiver.SplitUp())
            {
                Task.Run(async () =>
                {
                    try
                    {
                        await Bot.SendTextMessageAsync(id.Trim(), $"{subject} \n\n {message}");
                        Logger.Info($"Telegram message sent to {id} id successful.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Fatal(ex, $"Send telegram failed for telegram id: {id}");
                        completed = false;
                    }
                });
            }

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
                    tasks.Add(Bot.SendTextMessageAsync(id.Trim(), $"{subject} \n\n {message}"));
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

        protected void Bot_OnMessage(object sender, global::Telegram.Bot.Args.MessageEventArgs e)
        {
            Logger.Info($"The {e.Message.From.Username} user call notification bot by message: {e.Message.Text}");
            Bot.SendTextMessageAsync(e.Message.From.Id, e.Message.From.Id.ToString());
            Logger.Info($"Response to user by user id: {e.Message.From.Id}");
        }
    }
}