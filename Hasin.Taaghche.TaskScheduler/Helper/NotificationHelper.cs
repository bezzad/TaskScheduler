using System;
using System.Diagnostics;
using System.Reflection;
using Hasin.Taaghche.TaskScheduler.NotificationServices;
using Hasin.Taaghche.TaskScheduler.NotificationServices.CallRestApi;
using Hasin.Taaghche.TaskScheduler.NotificationServices.Email;
using Hasin.Taaghche.TaskScheduler.NotificationServices.Slack;
using Hasin.Taaghche.TaskScheduler.NotificationServices.SMS;
using Hasin.Taaghche.TaskScheduler.NotificationServices.Telegram;
using Hasin.Taaghche.TaskScheduler.Properties;

namespace Hasin.Taaghche.TaskScheduler.Helper
{
    public static class NotificationHelper
    {
        public static INotificationService Factory(this NotificationType notifyType)
        {
            switch (notifyType)
            {
                case NotificationType.Sms:
                    return new SmsService(
                        userName: Settings.Default.RahyabUserName,
                        password: Settings.Default.RahyabPassword,
                        sendNumber: Settings.Default.RahyabSendNumber);

                case NotificationType.Email:
                    return new EmailService(
                       userName: Settings.Default.SmtpUrl,
                       password: Settings.Default.SmtpSenderPassword,
                       senderEmail: Settings.Default.SmtpSenderMail);

                case NotificationType.Telegram:
                    return new TelegramService(
                        userName: Settings.Default.TelegramBotUsername,
                        apiKey: Settings.Default.TelegramBotApiKey,
                        senderBot: Settings.Default.TelegramBotUsername);

                case NotificationType.Slack:
                    var asmInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                    return new SlackService(
                        userName: asmInfo.ProductName + " v" + asmInfo.ProductVersion,
                        webhookUrl: Settings.Default.SlackWebhookUrl,
                        icon: Settings.Default.SlackIconUrl);

                case NotificationType.CallRestApi:
                    return new CallRestApiService(
                        clientId: Settings.Default.ApiClientId,
                        clientSecret: Settings.Default.ApiClientSecret,
                        authServerUrl: Settings.Default.ApiAuthServerUrl)
                    {
                        AuthScope = Settings.Default.ApiAuthScope,
                        AuthGrantType = Settings.Default.ApiAuthGrantType
                    };

                default:
                    throw new ArgumentOutOfRangeException(nameof(notifyType), notifyType, null);
            }
        }
    }
}