namespace CotpsBot.Models.Http
{
    public class BalanceResponse: BaseResponse
    {
        public BalanceInfo userinfo { get; set; }
        public BalanceDeal deal { get; set; }
    }
}