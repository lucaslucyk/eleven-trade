namespace CotpsBot.Models.Http
{
    public class ReceiveProfitResponse: BaseResponse
    {
        public string msg { get; set; }
        public int code { get; set; }
        public string balance { get; set; }
    }
}