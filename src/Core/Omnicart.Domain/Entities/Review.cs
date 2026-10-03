using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omnicart.Domain.Common;

namespace Omnicart.Domain.Entities
{
    public class Review : BaseEntity, IMustHaveTenant
    {
        public Guid TenantId { get; set; }
        public Guid ProductId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;
        public Product? Product { get; set; }
    }
}
