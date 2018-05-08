using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Hasin.Taaghche.TaskScheduler.Helper;
using Hasin.Taaghche.TaskScheduler.Model;
using Hasin.Taaghche.TaskScheduler.NotificationServices;
using Newtonsoft.Json.Linq;
using NLog;
using RestSharp;

namespace Hasin.Taaghche.TaskScheduler.Core
{
    public static class ActionRunner
    {
        #region Properties

        private static readonly Logger Nlogger = LogManager.GetCurrentClassLogger();

        #endregion

        #region Action Methods

        public static string CallRestApi(
            string url,
            string httpMethod,
            IDictionary<string, string> headers,
            IDictionary<string, string> queryParameters,
            IDictionary<string, string> parameters,
            string body)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return "The url is empty!";
                var client = new RestClient(url);

                if (string.IsNullOrEmpty(httpMethod)) httpMethod = "GET";
                var method = (Method) Enum.Parse(typeof(Method), httpMethod.ToUpper());
                var request = new RestRequest(method);

                var resourceUrl = "";

                if (headers?.Any() == true)
                    foreach (var head in headers)
                        request.AddHeader(head.Key, head.Value); // adds URL headers

                if (queryParameters?.Any() == true)
                {
                    resourceUrl += "?";

                    foreach (var query in queryParameters)
                    {
                        request.AddQueryParameter(query.Key,
                            query.Value); // adds URL querystring like: baseUrl/?name=value&name2=value2
                        resourceUrl += $"{query.Key}={query.Value}&";
                    }

                    resourceUrl = resourceUrl.Substring(0, resourceUrl.Length - 1);
                }

                if (parameters?.Any() == true)
                    foreach (var parameter in parameters)
                        request.AddParameter(parameter.Key, parameter.Value,
                            ParameterType.RequestBody); // adds URL parameters based on Method

                // add HTTP Headers
                if (body != null)
                    request.AddJsonBody(body);

                Nlogger.Info($"{method}:   {url}/{resourceUrl}");

                // execute the request
                var response = client.Execute(request);
                var content = $"{response.StatusCode}: {response.Content}"; // raw content as string

                return content;
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
                return exp.Message;
            }
        }

        public static string KillProgress(string process)
        {
            var result = "Process killing status:\n\r";

            foreach (var proc in Process.GetProcessesByName(process))
                try
                {
                    proc.Kill();
                    result += $"{proc.Id}: {proc.ProcessName} killed at {DateTime.Now:s}";
                }
                catch (Exception exp)
                {
                    Nlogger.Fatal(exp);
                    result += $"{proc.Id}: {proc.ProcessName} {exp.Message}";
                }
                finally
                {
                    result += Environment.NewLine;
                }

            return result;
        }

        public static string SendEmail(string receiver, string message, string subject)
        {
            try
            {
                Nlogger.Info($"Sending email to [{receiver}] ...");
                var emailService = NotificationType.Email.Factory();
                var result = emailService.Send(receiver, message, subject);
                Nlogger.Info($"Send email successfully completed to {receiver} by result message: {result.Message}");
                return result.Message;
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
                return exp.Message;
            }
        }

        public static string SendSms(string receiver, string message, string subject = null)
        {
            try
            {
                Nlogger.Info($"Sending SMS to [{receiver}] ...");
                var smsService = NotificationType.Sms.Factory();
                var result = smsService.Send(receiver, message, subject);
                Nlogger.Info($"Send SMS successfully completed to {receiver} by result message: {result.Message}");
                return result.Message;
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
                return exp.Message;
            }
        }

        public static string StartProgram(
            string fileName,
            string arguments,
            string windowsStyle,
            bool createNoWindow,
            bool useShellExecute)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    throw new FileNotFoundException("The file path is empty!", fileName);

                fileName = fileName.Replace(@"\\", @"\");
                arguments = arguments?.Replace(@"\\", @"\");

                var style = string.IsNullOrEmpty(windowsStyle)
                    ? ProcessWindowStyle.Normal
                    : (ProcessWindowStyle) Enum.Parse(typeof(ProcessWindowStyle), windowsStyle, true);

                // Prepare the process to run
                var start = new ProcessStartInfo
                {
                    // Enter in the command line arguments, everything you would enter after the executable name itself
                    Arguments =
                        Environment.ExpandEnvironmentVariables(
                            arguments ?? ""), // Replace specifial folders name by real paths
                    //
                    // Enter the executable to run, including the complete path
                    FileName =
                        Environment.ExpandEnvironmentVariables(
                            fileName), // Replace specifial folders name by real paths
                    //
                    // Do you want to show a console window?
                    WindowStyle = style,
                    CreateNoWindow = createNoWindow,
                    UseShellExecute = useShellExecute
                };

                Nlogger.Info($"Starting [{fileName}] ...");
                var proc = Process.Start(start);
                return proc == null
                    ? $"Can not start [{fileName}]!"
                    : $"Started [{proc.ProcessName}] process by id: {proc.Id}";
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
                return exp.Message;
            }
        }

        #endregion

        #region Core Methods

        public static string Run(this IJob job)
        {
            Nlogger.Info($"Runing `{job.ActionName}({job.Name})` action ...");

            try
            {
                var methods = typeof(ActionRunner).GetMethods(BindingFlags.Static | BindingFlags.Public);
                var targetMethod =
                    methods.FirstOrDefault(m =>
                        string.Equals(m.Name, job.ActionName, StringComparison.OrdinalIgnoreCase));

                if (targetMethod == null) throw new MissingMethodException(nameof(ActionRunner), job.ActionName);

                // sort args according by method parameters orders
                var targetMethodParameters = targetMethod.GetParameters();
                var orderedArgs = new object[targetMethodParameters.Length];
                for (var i = 0; i < targetMethodParameters.Length; i++)
                    if (job.ActionParameters.ContainsKey(targetMethodParameters[i].Name))
                    {
                        // The parameter provided by given args
                        var obj = job.ActionParameters[targetMethodParameters[i].Name];
                        if (obj?.GetType() == typeof(JObject))
                            orderedArgs[i] = ((JObject) obj).ToObject(targetMethodParameters[i].ParameterType);
                        else orderedArgs[i] = obj;
                    }
                    else if (targetMethodParameters[i].HasDefaultValue) // for optional parameter
                    {
                        orderedArgs[i] = targetMethodParameters[i].DefaultValue;
                    }
                    else // create default value
                    {
                        orderedArgs[i] = targetMethodParameters[i].ParameterType.GetDefault();
                    }

                var result = targetMethod.Invoke(null, orderedArgs);

                Nlogger.Info($"Result of {job.ActionName}({job.Name}):   {result}");
                Nlogger.Info($"The `{job.ActionName}({job.Name})` action completed\n");

                return result?.ToString();
            }
            catch (Exception exp)
            {
                Nlogger.Fatal(exp);
                return exp.Message;
            }
        }

        internal static object GetDefault(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        #endregion
    }
}