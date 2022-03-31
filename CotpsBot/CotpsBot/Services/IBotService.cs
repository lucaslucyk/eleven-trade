using System;
using System.Threading.Tasks;
using CotpsBot.Models;

namespace CotpsBot.Services
{
    public interface IBotService
    {
        void Start();
        void Stop();
        bool GetStatus();
        DateTime GetLastRun();
        Task<TransactionsBalance> GetBalance();
    }
}