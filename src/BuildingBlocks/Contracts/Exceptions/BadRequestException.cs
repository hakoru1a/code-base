using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Exceptions
{
    public class BadRequestException : ApplicationException
    {
        public string ErrorCode { get; }
        public IDictionary<string, string[]> ValidationErrors { get; }

        public BadRequestException()
            : base("Bad request.")
        {
            ValidationErrors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string message)
            : base(message)
        {
            ValidationErrors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
            ValidationErrors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
            ValidationErrors = new Dictionary<string, string[]>();
        }

        public BadRequestException(string errorCode, string message, IDictionary<string, string[]> validationErrors)
            : base(message)
        {
            ErrorCode = errorCode;
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }

        public BadRequestException(string errorCode, string message, IDictionary<string, string[]> validationErrors, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
        }

        public static BadRequestException InvalidInput(string fieldName, string reason)
            => new BadRequestException("INVALID_INPUT", $"Invalid input for field '{fieldName}': {reason}");

        public static BadRequestException MissingRequiredField(string fieldName)
            => new BadRequestException("MISSING_REQUIRED_FIELD", $"Required field '{fieldName}' is missing.");

        public static BadRequestException InvalidFormat(string fieldName, string expectedFormat)
            => new BadRequestException("INVALID_FORMAT", $"Field '{fieldName}' has invalid format. Expected: {expectedFormat}");

        public static BadRequestException OutOfRange(string fieldName, object minValue, object maxValue)
            => new BadRequestException("OUT_OF_RANGE", $"Field '{fieldName}' is out of range. Must be between {minValue} and {maxValue}.");
    }
}
