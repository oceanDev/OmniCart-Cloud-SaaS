using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omnicart.Domain.Common;

namespace Omnicart.Domain.Entities
{
    public class ProductImage : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; set; }
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? AltText { get; set; } // SEO text
        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;
        public Product? Product { get; set; }
    }
}
