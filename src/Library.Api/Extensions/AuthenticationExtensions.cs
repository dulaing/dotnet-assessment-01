using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Library.Api.Security;
using Library.Application.Interfaces;
using Library.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
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
                        RoleClaimType = ClaimTypes.Role,

                        // without this a token stays usable for five minutes past its expiry
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context => WriteAuthProblemAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "authentication_required",
                            "A valid bearer token is required.",
                            context.HandleResponse),
                        OnForbidden = context => WriteAuthProblemAsync(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "forbidden",
                            "You do not have permission to perform this action.")
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.AdminOnly,
                    policy => policy.RequireRole(nameof(UserRole.Admin)));
            });

            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }

        // Returns the same machine-readable error shape for policy failures.
        private static Task WriteAuthProblemAsync(HttpContext httpContext, int status, string code, string detail, Action? beforeWrite = null)
        {
            beforeWrite?.Invoke();
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
            var problem = new ProblemDetails
            {
                Status = status,
                Title = status == StatusCodes.Status401Unauthorized ? "Unauthorized" : "Forbidden",
                Detail = detail
            };

            problem.Extensions["code"] = code;
            problem.Extensions["traceId"] = traceId;
            httpContext.Response.StatusCode = status;
            return httpContext.Response.WriteAsJsonAsync(problem);
        }
    }
}
