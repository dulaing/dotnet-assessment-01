using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Library.Api.Extensions
{
    // jwt validation setup lives here so Program.cs stays a short list of what is switched on
    public static class AuthenticationExtensions
    {
        // mirrors JwtTokenGenerator: whatever that signs with, this must validate against
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection("Jwt");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = settings["Issuer"],
                        ValidAudience = settings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings["Key"]!)),

                        // without this a token stays usable for five minutes past its expiry
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
