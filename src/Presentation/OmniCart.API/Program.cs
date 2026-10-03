using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OmniCart.API.Middlewares;
using OmniCart.Application;
using OmniCart.Infrustructure;

var builder = WebApplication.CreateBuilder(args);

// ১. লেয়ারগুলোর সার্ভিস রেজিস্ট্রেশন
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// ২. JWT Bearer Authentication কনফিগারেশন
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? "SuperSecretKeyForOmniCartCloudEnterpriseSaaSPlatform2026!#*%";
var issuer = jwtSettings["Issuer"] ?? "OmniCart.API";
var audience = jwtSettings["Audience"] ?? "OmniCart.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Dev mode
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ৩. কন্ট্রোলার ও Swagger যুক্ত করা (Swagger Bearer Auth সহ)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OmniCart Cloud SaaS API",
        Version = "v1",
        Description = "High-Standard Enterprise Multi-Tenant E-Commerce API with JWT, Postgres & Redis"
    });

    // Swagger UI-তে JWT Bearer Token ইনপুট দেওয়ার জন্য সিকিউরিটি ডেফিনিশন
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsIn...\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// ৪. CORS পলিসি
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// ৫. Swagger UI কনফিগারেশন
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OmniCart Cloud API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");

// ৬. টেন্যান্ট মিডলওয়্যার
app.UseMiddleware<TenantResolutionMiddleware>();

// ৭. অথেন্টিকেশন ও অথরাইজেশন
app.UseAuthentication();
app.UseAuthorization();

// ৮. কন্ট্রোলার ম্যাপ করা
app.MapControllers();

app.Run();
