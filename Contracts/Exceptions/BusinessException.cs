using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Exceptions
{
    public class BusinessException : ApplicationException
    {
        public string ErrorCode { get; }
        public object Details { get; }

        public BusinessException()
            : base("A business rule violation occurred.")
        {
        }

        public BusinessException(string message)
            : base(message)
        {
        }

        public BusinessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public BusinessException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public BusinessException(string errorCode, string message, object details)
            : base(message)
        {
            ErrorCode = errorCode;
            Details = details;
        }

        public BusinessException(string errorCode, string message, object details, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Details = details;
        }
    }
}
