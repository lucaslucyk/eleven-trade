using System;
using System.Threading.Tasks;
using CotpsBot.Models;

namespace CotpsBot.Services
{
    public interface IBotService
    {
        void Start();
        void Stop();
        Task Restart();
        bool GetStatus();
        bool GetBusyStatus();
        DateTime GetLastRun();
        Task<TransactionsBalance> GetBalance();
    }
}