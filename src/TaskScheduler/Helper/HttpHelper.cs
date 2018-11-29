using System.Web.Http;
using Newtonsoft.Json;
using RestSharp;

namespace TaskScheduler.Helper
{
    internal static class HttpHelper
    {
        public static bool IsSuccessful(this IRestResponse response)
        {
            // if status code was 2xx
            return (int) response.StatusCode / 100 == 2;
        }

        public static T ReadData<T>(this IRestResponse response)
        {
            if (!response.IsSuccessful())
                throw new HttpResponseException(response.StatusCode);

            return JsonConvert.DeserializeObject<T>(response.Content);
        }
    }
}