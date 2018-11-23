using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TaskScheduler.Helper;

namespace TaskScheduler.NotificationServices.Email
{
    public class EmailService : NotificationService
    {
        public new NotificationType NotificationType { get; } = NotificationType.Email;
        public string SmtpPassword { get; set; }
        public string SmtpSenderEmail { get; set; }
        public string SmtpUrl { get; set; }
        public int SmtpPort { get; set; }
        public string Logo { get; set; }
        public string LogoUrl { get; set; }
        public string ClickUrl { get; set; }
        public string UnsubscribeUrl { get; set; }
        protected SmtpClient Client { get; set; }

        public EmailService() { }
        public EmailService(string smtpUrl, string smtpPassword, int smtpPort, string smtpSenderEmail,
            string clickUrl = "#", string unsubscribeUrl = "#", string logo = null, string logoUrl = null, bool isDefaultService = false)
        {
            SmtpUrl = smtpUrl;
            SmtpPassword = smtpPassword;
            SmtpPort = smtpPort;
            SmtpSenderEmail = smtpSenderEmail;
            ClickUrl = clickUrl;
            UnsubscribeUrl = unsubscribeUrl;
            Logo = logo;
            LogoUrl = logoUrl;
            IsDefaultService = isDefaultService;
        }

        public void Initial()
        {
            if (Client == null)
            {
                Client = GetSmtpClient();
            }
        }

        public override SystemNotification Send(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver))
                return SystemNotification.InvalidOperation;

            foreach (var email in receiver.SplitUp())
                try
                {
                    if (string.IsNullOrEmpty(email) || !ValidateEmailAddressSyntax(email))
                    {
                        Logger.Warn($"The {email} email address is not valid!");
                        completed = false;
                    }

                    var mailMessage = GetMailMessage(email, message.CleanText(), subject.CleanText());

                    using (var smtpClient = GetSmtpClient())
                    {
                        smtpClient.Send(mailMessage);
                        Logger.Info($"Email Send successfully to: {email}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, $"Send email failed for sending mail to {email}");
                    completed = false;
                }

            return completed ? SystemNotification.SuccessfullyDone : SystemNotification.InternalError;
        }

        public override async Task<SystemNotification> SendAsync(string receiver, string message, string subject)
        {
            var completed = true;
            if (string.IsNullOrEmpty(receiver))
                return SystemNotification.InvalidOperation;

            foreach (var email in receiver.SplitUp())
                try
                {
                    if (string.IsNullOrEmpty(email) || !ValidateEmailAddressSyntax(email))
                    {
                        Logger.Warn($"The {email} email address is not valid!");
                        completed = false;
                    }

                    var mailMessage = GetMailMessage(email, message.CleanText(), subject.CleanText());

                    using (var smtpClient = GetSmtpClient())
                    {
                        await smtpClient.SendMailAsync(mailMessage);
                        Logger.Info($"Email Send successfully to: {email}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Fatal(ex, $"Send email failed for sending mail to {email}");
                    completed = false;
                }

            return completed ? SystemNotification.SuccessfullyDone : SystemNotification.InternalError;
        }

        public SmtpClient GetSmtpClient()
        {
            var smtpClient = new SmtpClient
            {
                Host = SmtpUrl,
                Port = SmtpPort,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
            if (SmtpSenderEmail != null)
                smtpClient.Credentials =
                    new NetworkCredential(SmtpSenderEmail, SmtpPassword);

            return smtpClient;
        }

        private MailMessage GetMailMessage(string receiver, string message, string subject)
        {
            // Construct the alternate body as HTML.
            var body = FileManager.ReadResourceFile("NotificationServices.Email.EmailTemplate.html");

            body = body
                ?.Replace("{message}", message?.Replace("\n", "<br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;"))
                .Replace("{subject}", subject.ToLowerCaseNamingConvention())
                .Replace("{title}", Assembly.GetEntryAssembly().GetName().Name.ToLowerCaseNamingConvention())
                .Replace("{unsubscribeUrl}", UnsubscribeUrl)
                .Replace("{clickUrl}", ClickUrl)
                .Replace("{receiver}", receiver.Replace(".", "_"))
                .Replace("{version}", Assembly.GetEntryAssembly().GetName().Version.ToString(3))
                .Replace("{emailDateTime}", DateTime.Now.ToString("F", CultureInfo.CreateSpecificCulture("en")));

            var mail = new MailMessage(SmtpSenderEmail, receiver)
            {
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                From = new MailAddress(SmtpSenderEmail),
                To = { receiver },
                Subject = $"{subject}  -  {DateTime.Now.ToString("g", CultureInfo.CreateSpecificCulture("en"))}",
                Body = body ?? message.Replace(@"\n", "<br/>"),
                IsBodyHtml = true,
                Priority = MailPriority.High
            };

            if (Logo != null)
            {
                var imgStream = new MemoryStream(Convert.FromBase64String(Logo));
                var attachment = new Attachment(imgStream, "logo.png")
                {
                    ContentId = "logo.png",
                    TransferEncoding = TransferEncoding.Base64
                };
                mail.Attachments.Add(attachment);
            }
            else if (LogoUrl != null)
            {
                mail.Body = mail.Body.Replace("cid:logo.png", LogoUrl);
            }

            return mail;
        }

        /// <summary>
        ///     Verifies if the specified string is a correctly formatted e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address string.</param>
        /// <returns><b>true</b> if the syntax of the specified string is correct; otherwise, <b>false</b>.</returns>
        /// <remarks>
        ///     This method only checks the syntax of the e-mail address. It does NOT make any network connections and thus
        ///     does NOT actually check if this address exists and you can send e-mails there.
        ///     <note>
        ///         As this method is static, you do not need to create an instance of
        ///         <see cref="T:NotificationServices.EmailService.EmailService" /> object in order to use it.
        ///     </note>
        /// </remarks>
        /// <example>
        ///     This example returns <i>Correct syntax</i> as <i>_mike.o'neil_loves_underscores@sub-domain.travel</i> address is
        ///     complex but still has valid syntax.
        ///     <code lang="C#">
        /// using NotificationServices.EmailService;
        /// 
        /// if (EmailService.ValidateEmailAddressSyntax("_mike.o'neil_loves_underscores@sub-domain.travel"))
        /// {
        /// 	Console.WriteLine("Correct syntax");
        /// }
        /// else
        /// {
        /// 	Console.WriteLine("Wrong syntax");
        /// }
        /// </code>
        /// </example>
        public static bool ValidateEmailAddressSyntax(string email)
        {
            if (email == null)
                throw new ArgumentNullException(nameof(email));

            return Regex.IsMatch(email,
                "^(([\\w]+['\\.\\-+])+[\\w]+|([\\w]+))@((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|([a-zA-Z0-9]+[\\w-]*\\.)+[a-zA-Z]{2,9})$");
        }
    }
}