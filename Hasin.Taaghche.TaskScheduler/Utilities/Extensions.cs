using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using RestSharp;

namespace Hasin.Taaghche.TaskScheduler.Utilities
{
    internal static class Extensions
    {
        public static bool IsSuccessful(this IRestResponse response)
        {
            // if status code was 2xx
            return (int)response.StatusCode / 100 == 2;
        }
        public static T ReadData<T>(this IRestResponse response)
        {
            if (!response.IsSuccessful())
                throw new HttpResponseException(response.StatusCode);

            return JsonConvert.DeserializeObject<T>(response.Content);
        }
    }
}
