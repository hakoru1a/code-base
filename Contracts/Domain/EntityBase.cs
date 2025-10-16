using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts.Domain.Interface;

namespace Contracts.Domain
{
    public abstract class EntityBase<T> : IEnityBase<T>
    {
        public T Id { get; set; } = default!;
    }
}
