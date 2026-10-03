using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OmniCart.Application.Common.Interfaces;
using Omnicart.Domain.Common;
using Omnicart.Domain.Entities;
using OmniCart.Infrustructure.Services;

namespace OmniCart.Infrustructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentTenantService _currentTenantService;
    public Guid? CurrentTenantId => _currentTenantService.TenantId;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options, 
        ICurrentTenantService currentTenantService) : base(options)
    {
        _currentTenantService = currentTenantService;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ০. PostgreSQL AI pgvector এক্সটেনশন এনাবল করা
        modelBuilder.HasPostgresExtension("vector");

        // ১. ডেসিমাল প্রিসিশন রুল (decimal ও decimal? উভয়ের জন্য 18,2)
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("decimal(18,2)");
        }

        // ২. মাল্টি-টেন্যান্ট ফিল্টার রুল
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IMustHaveTenant).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(ConvertFilterExpression(entityType.ClrType));
            }
        }

        // ৩. Multiple Cascade Paths প্রতিরোধে গ্লোবাল Restrict রুল
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }

        modelBuilder.Entity<Tenant>()
                    .HasIndex(t => t.Subdomain)
                    .IsUnique();

        // User Indexes
        modelBuilder.Entity<User>()
                    .HasIndex(u => new { u.TenantId, u.Email })
                    .IsUnique();

        // RefreshToken Indexes
        modelBuilder.Entity<RefreshToken>()
                    .HasIndex(r => r.Token)
                    .IsUnique();

        modelBuilder.Entity<Product>()
                    .HasIndex(p => new { p.TenantId, p.Slug })
                    .IsUnique();

        modelBuilder.Entity<Product>()
                    .HasIndex(p => new { p.TenantId, p.SKU })
                    .IsUnique();

        modelBuilder.Entity<Product>()
                    .Property(p => p.RowVersion)
                    .IsRowVersion();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // ১. অটোমেটিক CreatedAtUtc ও UpdatedAtUtc টাইমস্ট্যাম্প বসানো
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        // ২. নতুন ডেটা সেভ করার সময় স্বয়ংক্রিয়ভাবে CurrentTenantId বসিয়ে দেওয়া
        foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                if (CurrentTenantId.HasValue)
                {
                    entry.Entity.TenantId = CurrentTenantId.Value;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private LambdaExpression ConvertFilterExpression(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var tenantProperty = Expression.Property(parameter, nameof(IMustHaveTenant.TenantId));
        var tenantGuidNullable = Expression.Convert(tenantProperty, typeof(Guid?));

        var currentTenantProperty = Expression.Property(
            Expression.Constant(this),
            nameof(this.CurrentTenantId));

        var compareTenant = Expression.Equal(tenantGuidNullable, currentTenantProperty);

        return Expression.Lambda(compareTenant, parameter);
    }
}
