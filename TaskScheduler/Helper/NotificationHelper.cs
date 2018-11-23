using System;
using System.Linq;
using TaskScheduler.Core;
using TaskScheduler.NotificationServices;

namespace TaskScheduler.Helper
{
    public static class NotificationHelper
    {
        public static INotificationService Factory(this NotificationType notificationType)
        {
            var service = JobsManager.Setting.NotificationServices.FirstOrDefault(ns =>
                ns.IsDefaultService && ns.NotificationType == notificationType);

            if (service == null)
                throw new ArgumentOutOfRangeException(nameof(notificationType), notificationType, null);

            return service;

        }
    }
}