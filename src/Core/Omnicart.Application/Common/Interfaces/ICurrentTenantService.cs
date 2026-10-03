using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniCart.Application.Common.Interfaces
{
    public interface ICurrentTenantService
    {
        Guid? TenantId { get; }
        string? Subdomain { get; }
        void SetTenant(Guid tenantId, string subdomain);
    }
}
