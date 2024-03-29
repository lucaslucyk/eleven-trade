﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CotpsBot.Models.Http;

namespace CotpsBot.Services.Http
{
    public interface IRequestService
    {
        Task<TResult> GetAsync<TResult>(string uri, string token = "");
        Task<TResult> GetAsync<TResult>(string uri, IEnumerable<KeyValuePair<string, object>> parameters);
        Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data);
        Task<LoginResponse> LoginAsync(LoginRequest form);
        Task<BalanceResponse> GetBalance();
        Task<TeamInfoResponse> GetTeamInfo(int type, int page = 1, int size = 20);
        Task<ReceiveProfitResponse> ReceiveProfit(ReceiveProfitRequest data);
        Task<ReceiveProfitResponse> ReceiveProfit(int type);
        Task<OrderCreateResponse> CreateOrder();
        Task<OrderConfirmResponse> ConfirmOrder(string orderId);
        void Logout();
    }
}