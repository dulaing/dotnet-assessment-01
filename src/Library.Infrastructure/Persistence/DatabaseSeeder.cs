using Library.Application.Interfaces;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Infrastructure.Persistence
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAdminAsync(this IServiceProvider services)
        {
            // repositories are scoped, so startup code has to open its own scope to resolve them
            using var scope = services.CreateScope();

            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var email = configuration["Seed:AdminEmail"];
            var password = configuration["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return;
            }

            var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            if (await users.GetByEmailAsync(email, CancellationToken.None) is not null)
            {
                return;
            }

            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            await users.AddAsync(new User
            {
                Email = email,
                PasswordHash = hasher.Hash(password),
                Role = UserRole.Admin,
                MemberId = null
            }, CancellationToken.None);

            await users.SaveChangesAsync(CancellationToken.None);
        }
    }
}