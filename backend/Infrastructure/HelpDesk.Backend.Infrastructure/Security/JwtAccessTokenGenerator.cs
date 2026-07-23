using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HelpDesk.Backend.Application.Abstractions;
using HelpDesk.Backend.Domain.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HelpDesk.Backend.Infrastructure.Security;

internal sealed class JwtAccessTokenGenerator(
    IOptions<JwtOptions> options,
    IClock clock) : IAccessTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public AccessTokenResult Generate(User user)
    {
        var issuedAtUtc = clock.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_options.AccessTokenMinutes);
        var header = Base64UrlEncoder.Encode(
            Encoding.UTF8.GetBytes("""{"alg":"HS256","typ":"JWT"}"""));
        var payloadValues = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
            ["nameid"] = user.Id.ToString(),
            [JwtRegisteredClaimNames.Name] = user.FullName,
            [JwtRegisteredClaimNames.Email] = user.Email.Value,
            ["role"] = user.Role.ToString(),
            [JwtRegisteredClaimNames.Iss] = _options.Issuer,
            [JwtRegisteredClaimNames.Aud] = _options.Audience,
            [JwtRegisteredClaimNames.Iat] = EpochTime.GetIntDate(issuedAtUtc.UtcDateTime),
            [JwtRegisteredClaimNames.Nbf] = EpochTime.GetIntDate(issuedAtUtc.UtcDateTime),
            [JwtRegisteredClaimNames.Exp] = EpochTime.GetIntDate(expiresAtUtc.UtcDateTime)
        };
        var payload = Base64UrlEncoder.Encode(
            JsonSerializer.SerializeToUtf8Bytes(payloadValues));
        var unsignedToken = $"{header}.{payload}";
        using var algorithm = new HMACSHA256(
            Encoding.UTF8.GetBytes(_options.SigningKey));
        var signature = Base64UrlEncoder.Encode(
            algorithm.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));

        return new AccessTokenResult(
            $"{unsignedToken}.{signature}",
            expiresAtUtc);
    }
}
