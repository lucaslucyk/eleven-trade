using System;

namespace CotpsBot.Exceptions
{
    public class InternetException: Exception
    {
        public InternetException() { }
        public InternetException(string message): base(message){ }
    }
}