using System;

namespace WpfData.Requests
{
    internal class BadRequestConfigException : Exception
    {
        public BadRequestConfigException (string message, Exception innerException) : base(message, innerException)
        {
        }


    }
}
