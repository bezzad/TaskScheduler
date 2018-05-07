using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Hasin.Taaghche.TaskScheduler.NotificationServices.SMS
{
    public class CellphoneNumber
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private static readonly ConcurrentDictionary<string, List<DateTime>> SendHistory = new ConcurrentDictionary<string, List<DateTime>>();

        public string PhoneNumber { get; }

        public CellphoneNumber(string phoneNumber)
        {
            phoneNumber = phoneNumber.Trim();

            long tel;
            if (!long.TryParse(phoneNumber, out tel))
                throw new Exception("phone number should just contain numbers");

            if (phoneNumber.StartsWith("+98"))
                phoneNumber = "0" + phoneNumber.Substring(3);
            else if (phoneNumber.StartsWith("98"))
                phoneNumber = "0" + phoneNumber.Substring(2);
            else if (phoneNumber.StartsWith("0098"))
                phoneNumber = "0" + phoneNumber.Substring(4);
            else if (!phoneNumber.StartsWith("0"))
                phoneNumber = "0" + phoneNumber;

            if (phoneNumber.Length != 11)
                throw new Exception("phone number length is not 11");

            if (!phoneNumber.StartsWith("09"))
                throw new Exception("phone number dos not start with 09");

            PhoneNumber = phoneNumber;
        }

        public bool IsSendingLimitExceeded()
        {
            List<DateTime> history;
            if (SendHistory.ContainsKey(PhoneNumber))
            {
                history = SendHistory[PhoneNumber];
            }
            else
            {
                history = new List<DateTime>();
                SendHistory[PhoneNumber] = history;
            }

            if (history.Count(d => d > DateTime.Now.AddMinutes(-2)) >= 3)
            {
                Logger.Warn($"MinuteSendingLimitExceeded for {PhoneNumber}");
                return true;
            }
            if (history.Count(d => d > DateTime.Now.AddHours(-1)) >= 6)
            {
                Logger.Warn($"HourSendingLimitExceeded for {PhoneNumber}");
                return true;
            }
            if (history.Count(d => d > DateTime.Now.AddDays(-1)) >= 20)
            {
                Logger.Warn($"DaySendingLimitExceeded for {PhoneNumber}");
                return true;
            }

            history.Add(DateTime.Now);
            return false;
        }

        public static CellphoneNumber Normalize(string number)
        {
            CellphoneNumber cellphone;
            try
            {
                cellphone = new CellphoneNumber(number);
            }
            catch (Exception exp)
            {
                Logger.Warn($"Invalid send SMS request received. {exp.Message}");
                return null;
            }
            return cellphone;
        }
    }
}
