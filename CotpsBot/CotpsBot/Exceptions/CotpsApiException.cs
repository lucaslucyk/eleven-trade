using System;

namespace CotpsBot.Exceptions
{
    public class ApiException: Exception
    {
        public ApiException() { }
        public ApiException(string message): base(message){ }
    }
}