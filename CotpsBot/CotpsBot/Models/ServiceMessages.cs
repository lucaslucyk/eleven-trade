using System;

namespace CotpsBot.Models
{
    public class ServiceMessage
    {
        public bool IsRunning { get; set; }
        public DateTime LastRun { get; set; }
    }
}