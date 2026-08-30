using Library.Application.Common;
using Library.Domain.Entities;

namespace Library.Application.Interfaces
{
    public interface ITokenGenerator
    {
        AccessToken Generate(User user);
    }
}