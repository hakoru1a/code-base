using Contracts.Domain.Enums;
using Contracts.Domain.Interface;
using Contracts.Exceptions;

namespace Contracts.Domain
{
    public abstract class EntityBase<T> : IEnityBase<T>
    {
        public T Id { get; set; } = default!;
        public EntityStatus? Status { get; set; } = EntityStatus.Active;

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

        /// <summary>
        /// Checks if this entity satisfies the given specification.
        /// This method is available to all entities that inherit from EntityBase.
        /// </summary>
        /// <typeparam name="TEntity">The concrete entity type (must be the same as or derived from this entity)</typeparam>
        /// <param name="specification">The specification to check</param>
        /// <returns>True if the specification is satisfied, false otherwise</returns>
        public bool SatisfiesSpecification<TEntity>(ISpecification<TEntity> specification) where TEntity : EntityBase<T>
        {
            if (this is TEntity entity)
            {
                return specification.IsSatisfiedBy(entity);
            }
            return false;
        }

    }
}
