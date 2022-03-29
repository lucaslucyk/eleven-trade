using System.Collections.Generic;
using System.Threading.Tasks;

namespace CotpsBot.Services.Http
{
    public interface IRequestService
    {
        Task<TResult> GetAsync<TResult>(string uri, string token = "");
        Task<TResult> GetAsync<TResult>(string uri, IEnumerable<KeyValuePair<string, object>> parameters);
        Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data, string token = "");
    }
}