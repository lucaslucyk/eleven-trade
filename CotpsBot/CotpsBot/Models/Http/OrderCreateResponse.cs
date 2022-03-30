namespace CotpsBot.Models.Http
{
    public class OrderCreateResponse: BaseResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
        public OrderCreateData data { get; set; }
    }
}