using System.Threading.Tasks;
using Hasin.Taaghche.Infrastructure.AuthenticationClient;
using Hasin.Taaghche.Probes.Properties;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace Hasin.Taaghche.Probes.Utilities
{
    class V2MsClient
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static async Task<T> ExecRequestWithAuthAsync<T>(string address, Method method, object jsonBody = null)
        {
            var response = await ExecRequestWithAuthResultAsync(address, method, jsonBody);
            return JsonConvert.DeserializeObject<T>(response);
        }

        public static async Task<string> ExecRequestWithAuthResultAsync(string address,
            Method method,
            object jsonBody = null)
        {
            var response = await ClientRequestHelper.ExecRequestWithAuthAsync(GetNewClient(), address, method, jsonBody);
            return response.Content;
        }

        private static TaaghcheRestClient GetNewClient()
        {
            return new TaaghcheRestClient(Settings.Default.MsUrlV2);
        }
    }
}
