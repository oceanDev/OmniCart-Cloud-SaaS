namespace Omnicart.Domain.Common;

public interface IMustHaveTenant
{
    public Guid TenantId { get; set; }
}
