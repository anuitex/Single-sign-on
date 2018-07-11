using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSO.API.Common
{
    public class ValidationException : ApplicationException
    {
        public ValidationException(string message):base(message)
        {

        }
    }
}
