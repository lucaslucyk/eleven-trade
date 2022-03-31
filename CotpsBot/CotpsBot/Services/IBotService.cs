using System;

namespace CotpsBot.Services
{
    public interface IBotService
    {
        void Start();
        void Stop();
        bool GetStatus();
        DateTime GetLastRun();
    }
}