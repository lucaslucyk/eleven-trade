﻿namespace CotpsBot.Models.Http
{
    public class LoginResponse: BaseResponse
    {
        public int code { get; set; }
        public string msg { get; set; }
        public UserInfo userinfo { get; set; }
    }
}