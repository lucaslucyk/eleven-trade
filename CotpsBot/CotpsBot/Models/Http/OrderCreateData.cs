namespace CotpsBot.Models.Http
{
    public class OrderCreateData
    {
        public float min_price { get; set; }
        public float max_price { get; set; }
        public string price { get; set; }
        public string fiat { get; set; }
        public string nickName { get; set; }
        public string orderId { get; set; }
    }
}