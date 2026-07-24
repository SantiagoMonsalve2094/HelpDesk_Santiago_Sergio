using HelpDesk.Backend.Domain.Aggregates.Users;
using HelpDesk.Backend.Application.DTOs.Auth;

namespace HelpDesk.Backend.Application.Interfaces;

public interface IAccessTokenGenerator
{
    AccessTokenResult Generate(User user);
}
