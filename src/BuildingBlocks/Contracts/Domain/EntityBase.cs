using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Domain.Interface;
using Contracts.Exceptions;

namespace Contracts.Domain
{
    public abstract class EntityBase<T> : IEnityBase<T>
    {
        public T Id { get; set; } = default!;

        /// <summary>
        /// Checks a business rule and throws BusinessRuleValidationException if the rule is broken.
        /// This method is available to all entities that inherit from EntityBase.
        /// </summary>
        /// <param name="rule">The business rule to check</param>
        /// <exception cref="BusinessRuleValidationException">Thrown when the rule is broken</exception>
        protected static void CheckRule(IBusinessRule rule)
        {
            if (rule.IsBroken())
            {
                throw new BusinessRuleValidationException(rule);
            }
        }
    }
}
