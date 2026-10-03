using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Omnicart.Domain.Common;
using Omnicart.Domain.Enums;

namespace Omnicart.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string? CustomDomain { get; set; }
        public string OwnerEmail { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? PrimaryColor { get; set; } = "#4F46E5";
        public TenantStatus Status { get; set; } = TenantStatus.Trial;
        public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
        public DateTime? TrialEndsAtUtc { get; set; } = DateTime.UtcNow.AddDays(14);
        public string? StripeCustomerId { get; set; }
        public string? StripeSubscriptionId { get; set; }
    }
}
