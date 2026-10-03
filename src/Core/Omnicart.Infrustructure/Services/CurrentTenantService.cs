using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmniCart.Application.Common.Interfaces;

namespace OmniCart.Infrustructure.Services
{
    internal class CurrentTenantService : ICurrentTenantService
    {
        public Guid? TenantId { get; private set; }
        public string? Subdomain { get; private set; }
        public void SetTenant(Guid tenantId, string subdomain)
        {
            TenantId = tenantId;
            Subdomain = subdomain;
        }
    }
}
