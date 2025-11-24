using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Exceptions
{
    public class UnauthorizedException : ApplicationException
    {
        public string? ErrorCode { get; }

        public UnauthorizedException()
            : base("Unauthorized access.")
        {
            ErrorCode = null;
        }

        public UnauthorizedException(string message)
            : base(message)
        {
            ErrorCode = null;
        }

        public UnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = null;
        }

        public UnauthorizedException(string errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public UnauthorizedException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        public static UnauthorizedException InvalidCredentials()
            => new UnauthorizedException("INVALID_CREDENTIALS", "Invalid username or password.");

        public static UnauthorizedException TokenExpired()
            => new UnauthorizedException("TOKEN_EXPIRED", "Authentication token has expired.");

        public static UnauthorizedException InsufficientPermissions(string requiredPermission)
            => new UnauthorizedException("INSUFFICIENT_PERMISSIONS", $"Insufficient permissions. Required: {requiredPermission}");

        public static UnauthorizedException AccountLocked()
            => new UnauthorizedException("ACCOUNT_LOCKED", "Account has been locked due to multiple failed login attempts.");
    }
}
