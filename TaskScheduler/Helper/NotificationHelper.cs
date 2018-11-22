using System;
using System.Diagnostics;
using System.Reflection;
using TaskScheduler.NotificationServices;
using TaskScheduler.NotificationServices.CallRestApi;
using TaskScheduler.NotificationServices.Email;
using TaskScheduler.NotificationServices.Slack;
using TaskScheduler.NotificationServices.SMS;
using TaskScheduler.NotificationServices.Telegram;
using TaskScheduler.Properties;

namespace TaskScheduler.Helper
{
    public static class NotificationHelper
    {
        public static INotificationService Factory(this NotificationType notifyType)
        {
            switch (notifyType)
            {
                case NotificationType.Sms:
                    return new SmsService(
                        Settings.Default.RahyabUserName,
                        Settings.Default.RahyabPassword,
                        Settings.Default.RahyabSendNumber);

                case NotificationType.Email:
                    return new EmailService(
                        Settings.Default.SmtpUrl,
                        Settings.Default.SmtpSenderPassword,
                        Settings.Default.SmtpSenderMail);

                case NotificationType.Telegram:
                    return new TelegramService(
                        Settings.Default.TelegramBotUsername,
                        Settings.Default.TelegramBotApiKey,
                        Settings.Default.TelegramBotUsername);

                case NotificationType.Slack:
                    var asmInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                    return new SlackService(
                        asmInfo.ProductName + " v" + asmInfo.ProductVersion,
                        Settings.Default.SlackWebhookUrl,
                        Settings.Default.SlackIconUrl);

                case NotificationType.CallRestApi:
                    return new CallRestApiService(
                        Settings.Default.ApiClientId,
                        Settings.Default.ApiClientSecret,
                        Settings.Default.ApiAuthServerUrl)
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