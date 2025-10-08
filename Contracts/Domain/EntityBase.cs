using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domain
{
    public abstract class EntityBase<T> : Interface.IEnityBase<T>
    {
        public T Id { get; set; } = default!;
    }
}
