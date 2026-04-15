using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Clients.Entities;
using Clients.Enums;

namespace Clients.Services;

public interface IJwtProvider
{
    string GenerateToken(string email);
    string GenerateToken(User user);
    bool IsTokenValid(string token);
    bool IsTokenExpired(string token);
    string GetEmailFromToken(string token);
    IEnumerable<Claim> GetClaimsFromToken(string token);
    DateTime GetTokenExpiration(string token);
}

public class JwtProvider : IJwtProvider
{
    private readonly IConfiguration _configuration;

    public JwtProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string email)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMs = int.Parse(jwtSettings["ExpirationMs"] ?? "86400000");

        var key = Encoding.ASCII.GetBytes(secretKey!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddMilliseconds(expirationMs),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Genera un token JWT incluyendo los roles del usuario (equivalente al método de Java)
    /// </summary>
    public string GenerateToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMs = int.Parse(jwtSettings["ExpirationMs"] ?? "86400000");

        var key = Encoding.ASCII.GetBytes(secretKey!);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("userId", user.Id.ToString()),
            new Claim("nick", user.Nick)
        };

        // Añadir los roles del usuario como claims (guardados como números para validación correcta)
        // Los números se corresponden con RoleEnum: USER=1, PREMIUM=2, ADMIN=3
        // Similar a: userDetails.getAuthorities() en Java
        foreach (var role in user.Roles)
        {
            // Guardar como número en lugar de nombre para que sea reconocido correctamente
            claims.Add(new Claim(ClaimTypes.Role, ((int)role).ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMilliseconds(expirationMs),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public bool IsTokenValid(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            var key = Encoding.ASCII.GetBytes(secretKey!);
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = false, // We check expiration separately
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return validatedToken is JwtSecurityToken;
        }
        catch
        {
            return false;
        }
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                return true;

            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    public string GetEmailFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                throw new InvalidOperationException("Token inválido");

            // Intentar obtener del claim Email, si no existe usar NameIdentifier
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
            if (!string.IsNullOrEmpty(emailClaim?.Value))
                return emailClaim.Value;

            // Fallback a NameIdentifier (que contiene el email en la generación del token)
            var nameIdentifier = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(nameIdentifier?.Value))
                return nameIdentifier.Value;

            throw new InvalidOperationException("Ni Email ni NameIdentifier encontrados en el token");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al extraer email del token: {ex.Message}");
        }
    }

    /// <summary>
    /// Extrae todos los claims del token JWT
    /// Equivalente a: authentication.getCredentials() en Java
    /// </summary>
    public IEnumerable<Claim> GetClaimsFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                throw new InvalidOperationException("Token inválido");

            return jwtToken.Claims;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al extraer claims del token: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene la fecha de expiración del token JWT
    /// Usada por el TokenBlacklistService para saber cuánto tiempo mantener el token invalidado
    /// </summary>
    public DateTime GetTokenExpiration(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
                throw new InvalidOperationException("Token inválido");

            return jwtToken.ValidTo;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al extraer expiración del token: {ex.Message}");
        }
    }
}
