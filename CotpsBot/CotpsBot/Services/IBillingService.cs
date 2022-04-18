using System.Collections.Generic;
using System.Threading.Tasks;
using CotpsBot.Models;
using Plugin.InAppBilling;

namespace CotpsBot.Services
{
    public interface IBillingService
    {
        bool IsConnected { get; }
        Task Connect();
        Task Disconnect();
        Task<bool> CheckBuy(bool autoPurchaseAcknowledge = true);
        Task<IEnumerable<InAppBillingProduct>?> GetAvailableSubs();
        Task<PurchaseResult> Purchase();
        Task<PurchaseResult> Purchase(string productId);
    }
}