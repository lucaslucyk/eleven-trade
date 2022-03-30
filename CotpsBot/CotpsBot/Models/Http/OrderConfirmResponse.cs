namespace CotpsBot.Models.Http
{
    public class OrderConfirmResponse: BaseResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
        public string balance { get; set; }
        public OrderConfirmData data { get; set; }
    }
}