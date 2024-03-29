﻿using System;
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
using CotpsBot.Helpers;
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
                // Debug.WriteLine("Can't create API Client.");
            }
        }

        #endregion
        
        #region Methods

        private static void SetupClient()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = Settings.APITimeOut
            };
            _client.DefaultRequestHeaders.Accept.Clear();
            //_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Connection.Clear();
            _client.DefaultRequestHeaders.Connection.Add("close");
        }

        public void Logout()
        {
            if (_client != null) _client.DefaultRequestHeaders.Authorization = null;
        }

        private static void SetToken(string token)
        {
            if (_client != null)
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
                if (tokens != null)
                {
                    var tokenList = tokens.ToList();
                    if (tokenList.Any())
                        SetToken(tokenList.First());
                }
            }
            
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync()
                    .ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // TODO: Handle 401 exception for expired `gateway_session` cookie or another code
                    throw new ApiException("COTPS API Unauthorized");
                }

                throw new ApiException(content);
            }
        }
        
        public async Task<TResult> GetAsync<TResult>(string uri, string token = "")
        {
            CheckConnection();
            var response = await _client.GetAsync(uri).ConfigureAwait(false);
            await HandleResponse(response);
            var serialized = await response.Content.ReadAsStringAsync();
            var result = await Task.Run(
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
            var response = await _client.PostAsync(
                Settings.APILoginUrl,
                new FormUrlEncodedContent(formData));
            
            await HandleResponse(response, true);
            
            var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return await Task.Run(() => JsonConvert.DeserializeObject<LoginResponse>(responseData)).ConfigureAwait(false);
        }

        public async Task<BalanceResponse> GetBalance()
        {
            return await GetAsync<BalanceResponse>(Settings.APIBalanceUrl);
        }

        public async Task<TeamInfoResponse> GetTeamInfo(int type, int page = 1, int size = 20)
        {
            var finalUri = $"{Settings.APITeamInfoUrl}?page={page}&size={size}&type={type}";
            return await GetAsync<TeamInfoResponse>(finalUri);
        }

        public async Task<ReceiveProfitResponse> ReceiveProfit(ReceiveProfitRequest data)
        {
            return await PostAsync<ReceiveProfitRequest, ReceiveProfitResponse>(Settings.APIReceiveProfitUrl, data);
        }
        public async Task<ReceiveProfitResponse> ReceiveProfit(int type)
        {
            return await ReceiveProfit(new ReceiveProfitRequest{type = type});
        }

        public async Task<OrderCreateResponse> CreateOrder()
        {
            return await GetAsync<OrderCreateResponse>(Settings.APIOrderCreateUrl);
        }

        public async Task<OrderConfirmResponse> ConfirmOrder(string orderId)
        {
            var finalUri = $"{Settings.APIOrderSubmitUrl}?orderId={orderId}";
            return await GetAsync<OrderConfirmResponse>(finalUri);
        }

        public async Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data)
        {
            CheckConnection();

            var serialized = await Task.Run(() => JsonConvert.SerializeObject(data))
            .ConfigureAwait(false);
            // empty response data to work 
            var responseData = string.Empty;

            if (string.IsNullOrWhiteSpace(responseData))
            {
                var response = await _client.PostAsync(
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