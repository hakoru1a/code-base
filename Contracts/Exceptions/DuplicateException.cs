using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Exceptions
{
    public class DuplicateException : ApplicationException
    {
        public DuplicateException()
            : base("Duplicate data found.")
        {
        }

        public DuplicateException(string message)
            : base(message)
        {
        }

        public DuplicateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DuplicateException(string entityName, string fieldName, object value)
            : base($"Duplicate {entityName} found. {fieldName} '{value}' already exists.")
        {
        }

        public DuplicateException(string entityName, string fieldName, object value, string additionalInfo)
            : base($"Duplicate {entityName} found. {fieldName} '{value}' already exists. {additionalInfo}")
        {
        }
    }
}
