using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CotpsBot.Exceptions;
using Newtonsoft.Json;
using OutsideWorks.Helpers;
using Xamarin.Essentials;

namespace CotpsBot.Services.Http
{
    public class RequestService : IRequestService
    {
        #region Fields

        private static readonly string BaseUrl = Settings.APIBaseUrl;
        private static HttpClient? _client;

        #endregion

        #region Methods

        private static void SetupClient()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
            _client.DefaultRequestHeaders.Accept.Clear();
            //_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Connection.Clear();
            _client.DefaultRequestHeaders.Connection.Add("close");
        }

        public static void SetToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        private static void CheckConnection()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                throw new InternetException("No Internet Connection");
        }
        
        private async Task HandleResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // TODO: Handle 401 exception for expired `gateway_session` cookie or another code
                    throw new Exception(content);
                }

                throw new HttpRequestException(content);
            }
        }
        
        public async Task<TResult> GetAsync<TResult>(string uri, string token = "")
        {
            // setupHttpClient(token);
            CheckConnection();
            HttpResponseMessage response = await _client.GetAsync(uri).ConfigureAwait(false);
            await HandleResponse(response);
            string serialized = await response.Content.ReadAsStringAsync();
            TResult result = await Task.Run(
                    () => JsonConvert.DeserializeObject<TResult>(serialized))
                .ConfigureAwait(false);
            return result;
        }

        public async Task<TResult> GetAsync<TResult>(string uri, IEnumerable<KeyValuePair<string, object>> parameters)
        {
            var keyValuePairs = parameters.ToList();
            if (!keyValuePairs.Any())
                return await GetAsync<TResult>(uri);
            
            var builder = new StringBuilder("?");
            
            var separator = "";
            foreach (var kvp in keyValuePairs.Where(kvp => kvp.Value != null))
            {
                builder.AppendFormat("{0}{1}={2}", separator, WebUtility.UrlEncode(kvp.Key), WebUtility.UrlEncode(kvp.Value.ToString()));
                separator = "&";
            }
            // get with params
            return await GetAsync<TResult>($"{uri}{builder.ToString()}");
        }
        
        public async Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data, string token = "")
        {
            // setupHttpClient(token);
            CheckConnection();
            
            string serialized = await Task.Run(() => JsonConvert.SerializeObject(data))
                .ConfigureAwait(false);
            // empty response data to work 
            var responseData = string.Empty;

            // check cache if not internet connection
            // try to get from cache
            // if (!String.IsNullOrWhiteSpace(cacheKey))
            // {
            //     if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            //         responseData = Barrel.Current.Get<string>(cacheKey);
            //     else if (!Barrel.Current.IsExpired(cacheKey))
            //         responseData = Barrel.Current.Get<string>(cacheKey);
            // }

            if (String.IsNullOrWhiteSpace(responseData))
            {
                HttpResponseMessage response = await _client.PostAsync(
                        uri,
                        new StringContent(serialized, Encoding.UTF8, "application/json"))
                    .ConfigureAwait(false);

                await HandleResponse(response);

                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // if (!String.IsNullOrWhiteSpace(cacheKey))
                //     Barrel.Current.Add(cacheKey, responseData, TimeSpan.FromMinutes(mins));
            }

            // if (refreshCookies) GetCookies(response);
            return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData)).ConfigureAwait(false);
        }

        #endregion
        
        
    }
}