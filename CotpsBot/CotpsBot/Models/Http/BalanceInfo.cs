namespace CotpsBot.Models.Http
{
    public class BalanceInfo
    {
        public string total_balance { get; set; }
        public string freeze_balance { get; set; }
        public string balance { get; set; }
        public int min_times { get; set; }
        public int max_times { get; set; }
        public int submit_min_times { get; set; }
        public int submit_max_times { get; set; }
        public int deal_min_balance { get; set; }
    }
}