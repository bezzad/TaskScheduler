using System;
using System.Net;
using System.Threading.Tasks;
using Hasin.Taaghche.Infrastructure.AuthenticationClient;
using RestSharp;

namespace Hasin.Taaghche.TaskScheduler.Utilities
{
    internal static class ClientRequestHelper
    {
        public static async Task<IRestResponse> ExecRequestWithAuthAsync(ITaaghcheRestClient client, string address,
            Method method,
            object jsonBody = null)
        {
            var response = await GetResponseWithAuthAsync(client, address, method, jsonBody);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var exception =
                    $"response failed for {address} ( status : {response.StatusCode} - content : {response.Content}";
                throw new Exception(exception);
            }

            return response;
        }

        private static Task<IRestResponse> GetResponseWithAuthAsync(ITaaghcheRestClient client, string address,
            Method method,
            object jsonBody = null,
            int? timeout = null)
        {
            var request = GenerateRequest(address, method, jsonBody);
            return ExecuteWithAuthorizationAsync(client, request, timeout);
        }

        private static Task<IRestResponse> ExecuteWithAuthorizationAsync(ITaaghcheRestClient client,
            IRestRequest request,
            int? timeout = null)
        {
            if (timeout.HasValue) client.Timeout = timeout.Value;
            return client.ExecuteWithAuthorizationAsync(request);
        }

        private static IRestRequest GenerateRequest(string address, Method method, object jsonBody = null)
        {
            if (method == Method.GET && jsonBody != null)
                throw new ArgumentException("Get request don't have jsonBody");

            RestRequest request;
            switch (method)
            {
                case Method.GET:
                    request = GetRequest(address);
                    break;
                case Method.POST:
                    request = PostRequest(address);
                    break;
                default:
                    request = new RestRequest(address, method);
                    break;
            }

            request.JsonSerializer = new RestSharpJsonNetSerializer();
            if (jsonBody != null) request.AddJsonBody(jsonBody);
            return request;
        }

        private static RestRequest GetRequest(string address)
        {
            return new RestRequest(address, Method.GET);
        }

        private static RestRequest PostRequest(string address)
        {
            return new RestRequest(address, Method.POST)
            {
                RequestFormat = DataFormat.Json
            };
        }
    }
}