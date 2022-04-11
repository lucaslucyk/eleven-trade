using System.Threading.Tasks;
using CotpsBot.Models;

namespace CotpsBot.Services
{
    public interface IBillingService
    {
        bool IsConnected { get; }
        Task Connect();
        Task Disconnect();
        Task<bool> CheckBuy();
        Task<PurchaseResult> Purchase();
    }
}