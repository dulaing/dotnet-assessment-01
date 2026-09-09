using System.Security.Claims;
using Library.Application.Interfaces;
using Library.Domain.Enums;

namespace Library.Api.Security
{
    // reads the claims that JwtTokenGenerator wrote, so the token stays the single source of identity
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUser(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

        public bool IsAdmin => Principal?.IsInRole(nameof(UserRole.Admin)) ?? false;

        public int? UserId
        {
            get
            {
                var value = Principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? Principal?.FindFirstValue("sub");
                return int.TryParse(value, out var userId) ? userId : null;
            }
        }

        public int? MemberId =>
            int.TryParse(Principal?.FindFirstValue("member_id"), out var memberId) ? memberId : null;
    }
}
