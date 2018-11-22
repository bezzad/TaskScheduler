﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TaskScheduler.Core;
using TaskScheduler.Helper;

namespace TaskScheduler.NotificationServices.CallRestApi
{
    [Guid("1bb06225-093f-0f47-ff10-e880d4d940bc")]
    public class CallRestApiService : NotificationService
    {
        public CallRestApiService(string clientId, string clientSecret, string authServerUrl)
            : base(clientId, clientSecret, authServerUrl)
        {
        }

        public string AuthGrantType { get; set; }
        public string AuthScope { get; set; }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            try
            {
                Logger.Info($"Notifying call rest api: {receiver}");
                if (receiver.StartsWith("POST", true, CultureInfo.CurrentCulture))
                {
                    var authToken = GetAuthToken();
                    var url = GetValidUrl(receiver);
                    var result = ActionRunner.CallRestApi(
                        url,
                        "POST",
                        new Dictionary<string, string>
                        {
                            {"cache-control", "no-cache"},
                            {"content-type", "application/x-www-form-urlencoded"},
                            {"taskScheduler-token", GetType().GUID.ToString()}
                        },
                        new Dictionary<string, string>
                        {
                            {"message", message},
                            {"subject", subject}
                        },
                        new Dictionary<string, string>
                        {
                            {"application/x-www-form-urlencoded", $"token={authToken}"}
                        },
                        null);

                    Logger.Info($"Post rest api successfully to: {url}");
                    return SystemNotification.SuccessfullyDone;
                }
                else // GET
                {
                    var url = GetValidUrl(receiver);
                    var result = ActionRunner.CallRestApi(
                        url,
                        "GET",
                        new Dictionary<string, string>
                        {
                            {"cache-control", "no-cache"},
                            {"taskScheduler-token", GetType().GUID.ToString()}
                        },
                        new Dictionary<string, string>
                        {
                            {"message", message},
                            {"subject", subject}
                        },
                        null,
                        null);

                    Logger.Info($"Get rest api successfully to: {url}");
                    return SystemNotification.SuccessfullyDone;
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, $"Notify call api failed for address: {receiver}");
                return SystemNotification.InternalError;
            }
        }

        public override Task<SystemNotification> SendAsync(string receiver, string message, string subject)
        {
            throw new NotImplementedException();
        }


        public string GetAuthToken()
        {
            try
            {
                var authHead = $"{UserName}:{Password}".EncodeToBase64();

                var token = ActionRunner.CallRestApi(
                    Sender,
                    "POST",
                    new Dictionary<string, string>
                    {
                        {"cache-control", "no-cache"},
                        {"authorization", $"Basic {authHead}"},
                        {"content-type", "application/x-www-form-urlencoded"},
                        {"taskScheduler-token", GetType().GUID.ToString()}
                    },
                    null,
                    new Dictionary<string, string>
                    {
                        {"application/x-www-form-urlencoded", $"grant_type={AuthGrantType}&scope={AuthScope}"}
                    },
                    null);

                var tokenKey = "access_token";
                if (token?.Contains(tokenKey) != true) return token;
                var iAccTok = token.IndexOf("{", StringComparison.OrdinalIgnoreCase);
                token = token.Substring(iAccTok, token.Length - iAccTok);
                token = token.Trim();
                var tokenObj = (JObject) JsonConvert.DeserializeObject(token);

                return tokenObj[tokenKey].ToString(Formatting.None).Replace("\"", "");
            }
            catch (Exception ex)
            {
                Logger.Fatal(ex, $"Can not give any authentication token from {Sender}");
                return null;
            }
        }

        private string GetValidUrl(string url)
        {
            url = url?.ToLower();

            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            if (!url.Contains("http")) return "http://" + url;
            if (url.StartsWith("http")) return url;

            var indexOfHttp = url.LastIndexOf("http", StringComparison.OrdinalIgnoreCase);
            return url.Substring(indexOfHttp, url.Length - indexOfHttp);
        }
    }
}