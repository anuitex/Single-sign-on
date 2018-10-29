using System;

namespace SSO.API.Common
{
    public class ValidationException : ApplicationException
    {
        public ValidationException(string message) : base(message)
        {

        }
    }
}
