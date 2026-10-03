using Microsoft.EntityFrameworkCore;
using OmniCart.Application.Common.Interfaces;
using OmniCart.Infrustructure.Persistence;

namespace OmniCart.API.Middlewares;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentTenantService currentTenantService,
        ApplicationDbContext dbContext)
    {
        string? subdomain = null;

        // ১. হেডার চেক (Postman/Testing-এর জন্য)
        if (context.Request.Headers.TryGetValue("X-Tenant", out var tenantHeader))
        {
            subdomain = tenantHeader.ToString();
        }
        else
        {
            // ২. হোস্টনেম থেকে সাবডোমেন বের করা (যেমন: apple.localhost)
            var host = context.Request.Host.Host;
            var parts = host.Split('.');

            if (parts.Length > 1 && parts[0] != "www" && parts[0] != "localhost")
            {
                subdomain = parts[0];
            }
        }

        // ৩. যদি সাবডোমেন থাকে এবং ডাটাবেজে পাওয়া যায়

        if (!string.IsNullOrEmpty(subdomain))
        {
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Subdomain.ToLower() == subdomain.ToLower());

            if (tenant != null)
            {
                currentTenantService.SetTenant(tenant.Id, tenant.Subdomain);
            }
        }

        await _next(context);
    }
}