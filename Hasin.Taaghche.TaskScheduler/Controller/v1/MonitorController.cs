using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Xml;
using Hasin.Taaghche.Infrastructure.AuthenticationClient;
using Hasin.Taaghche.Infrastructure.MotherShipModel;
using Hasin.Taaghche.Infrastructure.MotherShipModel.Report;
using Hasin.Taaghche.Payment.Defs;
using Hasin.Taaghche.TaskScheduler.Properties;
using Hasin.Taaghche.TaskScheduler.Utilities;
using RabbitMQ.Client;
using RestSharp;

namespace Hasin.Taaghche.TaskScheduler.Controller.v1
{
    /// <summary>
    ///     Monitor Web API Controller.
    /// </summary>
    /// <seealso>
    ///     <cref>V1.MonitorController{Infrastructure.MotherShipModel.MsInvoice, Core.Model.Wrapper.InvoiceWrapper}</cref>
    /// </seealso>
    [RoutePrefix("v1/monitor")]
    public class MonitorController : ApiController
    {
        /// <summary>
        ///     Monitor accounts from the specified duration,
        ///     weather is more than value count or not.
        /// </summary>
        /// <param name="duration">The duration is how minutes monitored from now to past.</param>
        /// <param name="min">The min is threshold of minimum account count.</param>
        /// <returns>
        ///     If no problem and accounts count is more than min value then get empty string,
        ///     and else get a message for alert them.
        /// </returns>
        [HttpGet]
        [Route("account")]
        public string Account(int duration, int min)
        {
            if (DateTime.Now.Hour < 8) return string.Empty;
            try
            {
                var response = new TaaghcheRestClient(Settings.Default.MsUrlV2)
                    .ExecuteWithAuthorization(new RestRequest("user/query/count", Method.POST)
                        .AddJsonBody(new MsAccountFilters
                        {
                            MinRegisterDate = DateTime.Now.AddMinutes(-duration),
                            MaxRegisterDate = DateTime.Now,
                            IsEnabled = true
                        })
                    );

                var count = response.ReadData<int>();

                if (count <= min)
                    return
                        $"Account registeration count between {DateTime.Now.AddMinutes(-duration)} " +
                        $"and {DateTime.Now} is {count} which is lower than {min}";
            }
            catch (Exception ex)
            {
                return $"Monitoring account register count failed : {ex.Message}";
            }

            return string.Empty;
        }

        /// <summary>
        ///     Monitor downloads from the specified duration,
        ///     weather is more than value count or not.
        /// </summary>
        /// <param name="duration">The duration is how minutes monitored from now to past.</param>
        /// <param name="value">The value is threshold of minimum download count.</param>
        /// <returns>
        ///     If no problem and downloaded count is more than value then get empty string,
        ///     and else get a message for alert them.
        /// </returns>
        [HttpGet]
        [Route("download")]
        public string Download(int duration, float value)
        {
            try
            {
                var response = new TaaghcheRestClient(Settings.Default.MsUrlV1)
                    .ExecuteWithAuthorization(new RestRequest("reports/dashboard", Method.GET)
                        .AddParameter("startDate", DateTime.Now.AddMinutes(-duration))
                        .AddParameter("endDate", DateTime.Now)
                    );
                var count = response.ReadData<RangeData<MsDashboardReport>>().Data.FullDownloads;

                response = new TaaghcheRestClient(Settings.Default.MsUrlV1)
                    .ExecuteWithAuthorization(new RestRequest("reports/dashboard", Method.GET)
                        .AddParameter("startDate", DateTime.Now.AddMinutes(-duration * 2))
                        .AddParameter("endDate", DateTime.Now.AddMinutes(-duration))
                    );
                var countPrev = response.ReadData<RangeData<MsDashboardReport>>().Data.FullDownloads;

                if (count < countPrev / value)
                    return $"Count: {count} - Previous Count: {countPrev} - Duration: {duration} - value: {value}";
            }
            catch (Exception ex)
            {
                return $"Monitoring download count failed : {ex.Message}";
            }

            return string.Empty;
        }

        /// <summary>
        ///     Monitor payments from the specified duration,
        ///     weather is more than min amount or not.
        /// </summary>
        /// <param name="duration">The duration is how minutes monitored from now to past.</param>
        /// <param name="min">Threshold of minimum payment amount.</param>
        /// <param name="method">The payment method name like "myket" or "taaghche" and etc.</param>
        /// <returns>
        ///     If no problem and payment amount is more than minimum amount then get empty string,
        ///     and else get a message for alert them.
        /// </returns>
        [HttpGet]
        [Route("payment")]
        public string Payment(int duration, int min, string method)
        {
            if (DateTime.Now.Hour < 8) return string.Empty;

            try
            {
                var filters = new PyInvoiceFilters
                {
                    Status = PyPaymentStatus.Successful,
                    FinishDateMin = DateTime.Now.AddMinutes(-duration),
                    FinishDateMax = DateTime.Now
                };
                switch (method.ToLower())
                {
                    case "myket":
                        filters.Method = PyPaymentMethod.Myket;
                        break;
                    case "mellat": // TODO change this for each payment
                        filters.Shaparak = true;
                        break;
                    case "taaghche":
                        filters.Method = PyPaymentMethod.Taaghche;
                        break;
                }

                var response = new TaaghcheRestClient(Settings.Default.PaymentServerUrl)
                    .ExecuteWithAuthorization(
                        new RestRequest("invoices/query/purchase/count", Method.POST).AddJsonBody(filters));
                var count = response.ReadData<int>();


                if (count <= min) return $"Count: {count}  {DateTime.Now.AddMinutes(-duration)} - {DateTime.Now}";
            }
            catch (Exception ex)
            {
                return $"Monitoring payment count failed : {ex.Message}, {ex.InnerException?.Message}";
            }

            return string.Empty;
        }

        /// <summary>
        ///     Monitor payments from the specified duration,
        ///     weather is more than max amount or not.
        /// </summary>
        /// <param name="duration">The duration is how minutes monitored from now to past.</param>
        /// <param name="maxTotal">Threshold of maximum total payment amount.</param>
        /// <param name="maxPerUser">Threshold of maximum per user payment amount.</param>
        /// <returns>
        ///     If no problem and payment amount is less than maximum amount then get empty string,
        ///     and else get a message for alert them.
        /// </returns>
        [HttpGet]
        [Route("highpayment")]
        public string HighPayment(int duration, int maxTotal, int maxPerUser)
        {
            var result = "";

            try
            {
                var fromDate = DateTime.Now.AddMinutes(-duration);
                var toDate = DateTime.Now;

                var filters = new PyInvoiceFilters
                {
                    Status = PyPaymentStatus.Successful,
                    FinishDateMin = fromDate,
                    FinishDateMax = toDate
                };

                var response = new TaaghcheRestClient(Settings.Default.PaymentServerUrl)
                    .ExecuteWithAuthorization(new RestRequest("invoices/report/GroupedByAccount", Method.POST)
                        .AddJsonBody(filters)
                    );
                var groupedReport = response.ReadData<PyGroupedReport>();
                var totalBuyCount = groupedReport.TotalBuyCount;
                var perUserCount = groupedReport.GroupedItems[0].BuyCount;

                if (totalBuyCount >= maxTotal)
                {
                    result = "Total payment count is more than normal limit in specified duration. \n";
                    result += $"TotalBuyCount: {totalBuyCount} is more than {maxTotal} from {fromDate} to {toDate} \n ";
                }

                if (perUserCount >= maxPerUser)
                {
                    result = "Some users have a payment count more than normal limit in specified duration. \n ";
                    result +=
                        $"PerUserBuyCount: {perUserCount} is more than {maxPerUser} from {fromDate} to {toDate} \n ";
                    foreach (var item in groupedReport.GroupedItems)
                        if (item.BuyCount >= maxPerUser)
                            result += $"user id: {item.Id} - buy count: {item.BuyCount} \n ";
                }
            }
            catch (Exception ex)
            {
                return $"Monitoring payment count failed : {ex.Message}, {ex.InnerException?.Message}";
            }

            return result;
        }

        /// <summary>
        ///     Monitor Site map fields last update.
        /// </summary>
        /// <param name="url">The site map address.</param>
        /// <param name="min">The minimum day for last update.</param>
        /// <returns>System.String.</returns>
        [HttpGet]
        [Route("Sitemap")]
        public string Sitemap(string url = @"https://taaghche.ir/sitemap/books.xml", int min = 1)
        {
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture =
                    CultureInfo.GetCultureInfo("en");

            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                var response = client.Execute(request);
                var sitemap = response?.Content;

                if (string.IsNullOrEmpty(sitemap))
                    return "Sitemap is empty";

                var xml = new XmlDocument();
                xml.LoadXml(sitemap); // suppose that sitemap contains "<url>...</url>"
                var urls = (xml["urlset"] ?? xml["sitemapindex"])?.ChildNodes;

                if (urls == null) return "Sitmap urls is empty!";

                foreach (XmlNode xn in urls)
                {
                    var loc = xn["loc"]?.InnerText;
                    var lastmod = xn["lastmod"]?.InnerText;
                    if (DateTime.TryParse(lastmod, out var date))
                    {
                        if (DateTime.Now.AddDays(-min).Date > date.Date)
                            return
                                $"Sitemap has at least one no updated field's from {DateTime.Now.AddDays(-min):d} to now. \n" +
                                $"It mean's the lastmod ({date:d}) of [{loc}] loc is more than sitemap update period (every {min} day).";
                    }
                    else
                    {
                        return
                            $"Can not read the lastmod of [{loc}] loc, may be that is invalid date format!";
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return $"Monitoring sitemap failed : {ex.Message}";
            }
        }

        /// <summary>
        ///     Monitor Rabbitmq uptime.
        /// </summary>
        /// <param name="topicName">name of topic that used for testing rabbitmq</param>
        /// <returns>System.String.</returns>
        [HttpGet]
        [Route("rabbitmq")]
        public string Rabbitmq(string topicName = "rmqtest")
        {
            try
            {
                var factory = new ConnectionFactory {HostName = "localhost"};
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(topicName,
                        durable: false,
                        type: "topic"
                    );

                    var body = Encoding.UTF8.GetBytes("Test RabbitMQ");

                    channel.BasicPublish(topicName,
                        topicName,
                        null,
                        body);
                }
            }
            catch (Exception ex)
            {
                return $"Monitoring RabbitMQ failed : {ex.Message}, {ex.InnerException?.Message}";
            }

            return string.Empty;
        }

        /// <summary>
        ///     Monitor Auth Service.
        /// </summary>
        /// <returns>System.String.</returns>
        [HttpGet]
        [Route("auth")]
        public string Auth()
        {
            try
            {
                var url = Settings.Default.AuthenticationServerUrl + ".well-known/jwks";
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                var response = client.Execute(request);
                var jwks = response?.Content;

                if (string.IsNullOrEmpty(jwks))
                    return "JWKS is empty";

                if (!jwks.Contains("keys"))
                    return "JWKS format has error";
            }
            catch (Exception ex)
            {
                return $"Monitoring jwks failed : {ex.Message}";
            }

            return string.Empty;
        }
    }
}