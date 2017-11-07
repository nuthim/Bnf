using System;

namespace Bnf.Serialization.Exceptions
{
    public class BnfValidationException : Exception
    {
        public BnfValidationException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
