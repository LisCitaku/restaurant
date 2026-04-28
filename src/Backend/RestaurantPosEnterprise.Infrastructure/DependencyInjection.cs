using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using RestaurantPosEnterprise.Application.Interfaces;
using RestaurantPosEnterprise.Infrastructure.Configuration;
using RestaurantPosEnterprise.Infrastructure.Data;
using RestaurantPosEnterprise.Infrastructure.Repositories;
using RestaurantPosEnterprise.Infrastructure.Security;
using RestaurantPosEnterprise.Infrastructure.Services;

namespace RestaurantPosEnterprise.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PrintingOptions>(configuration.GetSection(PrintingOptions.SectionName));
        services.Configure<KitchenTicketOptions>(configuration.GetSection(KitchenTicketOptions.SectionName));

        services.AddDbContext<RestaurantDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<RestaurantDbInitializer>();
        services.AddScoped<UserRepository>();
        services.AddScoped<TableRepository>();
        services.AddScoped<ProductRepository>();
        services.AddScoped<OrderRepository>();

        services.AddScoped<IPasswordHasherAdapter, PasswordHasherAdapter>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IKitchenTicketService, KitchenTicketService>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        return services;
    }
}
