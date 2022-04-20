namespace CotpsBot.Models.Http
{
    public class TeamInfoResponse: BaseResponse
    {
        public string msg { get; set; }
        public int pageSize { get; set; }
        public int code { get; set; }
        public TeamInfoData? data { get; set; }
    }
}