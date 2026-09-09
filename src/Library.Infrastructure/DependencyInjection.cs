using Library.Application.Interfaces;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Repositories;
using Library.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Infrastructure
{
    // persistence wiring lives in the layer that owns it, so Program.cs stays short
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<LibraryDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("LibraryDb")));

            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IBorrowingRepository, BorrowingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();
            services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

            // the db check lives here because this layer owns the DbContext
            services.AddHealthChecks().AddDbContextCheck<LibraryDbContext>("database");

            return services;
        }
    }
}
