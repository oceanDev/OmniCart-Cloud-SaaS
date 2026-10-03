using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omnicart.Domain.Common;

namespace Omnicart.Domain.Entities
{
    public class ProductVariant : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public decimal AdditionalPrice { get; set; } = 0;
        public int StockQuantity { get; set; } = 0;
        public Product? Product { get; set; }
    }
}
