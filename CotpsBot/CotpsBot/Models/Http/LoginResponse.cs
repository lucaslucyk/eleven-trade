namespace CotpsBot.Models.Http
{
    public class LoginResponse
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string msg { get; set; }
        public UserInfo userinfo { get; set; }
    }
}