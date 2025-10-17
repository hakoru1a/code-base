using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Common.Models
{
    public class ProductDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string SKU { get; set; } = string.Empty;
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset? LastModifiedDate { get; set; }
    }
}
