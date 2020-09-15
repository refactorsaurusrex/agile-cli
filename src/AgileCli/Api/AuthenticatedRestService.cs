using System;
using System.Net.Http;
using Refit;

namespace AgileCli.Api
{
    public class AuthenticatedRestService
    {
        public static T For<T>(string token, string baseUrl)
        {
            var client = new HttpClient(new AuthenticatedHttpClientHandler(token))
            {
                BaseAddress = new Uri(baseUrl)
            };

            return RestService.For<T>(client);
        }
    }
}