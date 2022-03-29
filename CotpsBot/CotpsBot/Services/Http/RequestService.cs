using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CotpsBot.Exceptions;
using CotpsBot.Models.Http;
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

        #region Constructor

        public RequestService()
        {
            try
            {
                SetupClient();
            }
            catch
            {
                // ignored
                Debug.WriteLine("Can't create API Client.");
            }
        }

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

        public void Logout()
        {
            _client.DefaultRequestHeaders.Authorization = null;
        }

        private static void SetToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        private static void CheckConnection()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                throw new InternetException("No Internet Connection");
        }
        
        private static async Task HandleResponse(HttpResponseMessage response, bool catchToken = false)
        {
            if (catchToken && response.IsSuccessStatusCode)
            {
                response.Headers.TryGetValues("Authorization", out var tokens);
                var tokenList = tokens.ToList();
                if (tokenList.Any())
                    SetToken(tokenList.First());
            }
            
            
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

        private static Dictionary<string, string> GetFormData(Models.Http.LoginRequest data)
        {
            return new Dictionary<string, string>
            {
                {"mobile", data.mobile},
                {"password", data.password},
                {"type", data.type}
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest form)
        {
            CheckConnection();
            var formData = GetFormData(form);
            HttpResponseMessage response = await _client.PostAsync(
                Settings.APILoginUrl,
                new FormUrlEncodedContent(formData));
            
            await HandleResponse(response, true);
            
            var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return await Task.Run(() => JsonConvert.DeserializeObject<LoginResponse>(responseData)).ConfigureAwait(false);
        }

        public async Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data)
        {
            CheckConnection();
            

            string serialized = await Task.Run(() => JsonConvert.SerializeObject(data))
            .ConfigureAwait(false);
            // empty response data to work 
            var responseData = string.Empty;

            if (String.IsNullOrWhiteSpace(responseData))
            {
                HttpResponseMessage response = await _client.PostAsync(
                        uri,
                        new StringContent(serialized, Encoding.UTF8, "application/json"))
                    .ConfigureAwait(false);

                await HandleResponse(response);

                responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return await Task.Run(() => JsonConvert.DeserializeObject<TResult>(responseData)).ConfigureAwait(false);
        }

        #endregion
        
        
    }
}