using Contracts.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Domain.Event.Product
{
    public record ProductCreatedEvent(
            long ProductId,
            string Name,
            string SKU,
            decimal Price,
            int Stock
        ) : BaseEvent;
}
