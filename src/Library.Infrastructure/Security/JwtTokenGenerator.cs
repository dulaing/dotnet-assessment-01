using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Library.Application.Common;
using Library.Application.Interfaces;
using Library.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Library.Infrastructure.Security
{
    public class JwtTokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AccessToken Generate(User user)
        {
            var settings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings["Key"]!));
            var expiresAt = DateTime.UtcNow.AddMinutes(int.Parse(settings["ExpiryMinutes"]!));

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            // only members carry this, and every ownership check in the api reads it
            if (user.MemberId is not null)
            {
                claims.Add(new Claim("member_id", user.MemberId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: settings["Issuer"],
                audience: settings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}