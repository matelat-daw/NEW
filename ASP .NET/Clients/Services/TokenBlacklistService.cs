using System.Collections.Concurrent;

namespace Clients.Services;

/// <summary>
/// Servicio para mantener una blacklist de tokens invalidados (logout)
/// Cuando un usuario hace logout, su token se agrega a la blacklist
/// El middleware valida contra esta lista antes de procesar requests
/// 
/// NOTA: En producción, esto debería estar en Redis o una base de datos
/// Para desarrollo, usamos ConcurrentDictionary en memoria (se pierde al reiniciar)
/// </summary>
public interface ITokenBlacklistService
{
    /// <summary>
    /// Añade un token a la lista negra (cuando el usuario hace logout)
    /// </summary>
    void BlacklistToken(string token, DateTime expiration);

    /// <summary>
    /// Verifica si un token está en la lista negra
    /// </summary>
    bool IsTokenBlacklisted(string token);

    /// <summary>
    /// Limpia tokens expirados de la lista negra (mantenimiento)
    /// </summary>
    void CleanupExpiredTokens();
}

public class TokenBlacklistService : ITokenBlacklistService
{
    // Almacenamiento en memoria: token -> fecha de expiración
    // En producción, esto sería Redis: SETEX blacklist:{token} {ttl} true
    private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens;
    private readonly ILogger<TokenBlacklistService> _logger;

    public TokenBlacklistService(ILogger<TokenBlacklistService> logger)
    {
        _blacklistedTokens = new ConcurrentDictionary<string, DateTime>();
        _logger = logger;
    }

    /// <summary>
    /// Añade un token a la blacklist cuando el usuario hace logout
    /// </summary>
    public void BlacklistToken(string token, DateTime expiration)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("⚠️ Intento de blacklist con token vacío");
            return;
        }

        // Usar un hash del token como clave (para no guardar el token completo)
        string tokenHash = HashToken(token);

        if (_blacklistedTokens.TryAdd(tokenHash, expiration))
        {
            _logger.LogInformation($"🚫 Token añadido a blacklist. Expirará en: {expiration:yyyy-MM-dd HH:mm:ss}");
        }
        else
        {
            // Si ya existe, actualizar la expiración
            _blacklistedTokens[tokenHash] = expiration;
            _logger.LogInformation($"🚫 Token actualizado en blacklist. Expirará en: {expiration:yyyy-MM-dd HH:mm:ss}");
        }
    }

    /// <summary>
    /// Verifica si un token está invalidado (blacklisted)
    /// </summary>
    public bool IsTokenBlacklisted(string token)
    {
        if (string.IsNullOrEmpty(token))
            return false;

        string tokenHash = HashToken(token);

        if (_blacklistedTokens.TryGetValue(tokenHash, out DateTime expiration))
        {
            // Si el token aún no ha expirado, está en blacklist
            if (expiration > DateTime.UtcNow)
            {
                _logger.LogWarning($"🚫 Token está en blacklist");
                return true;
            }
            else
            {
                // El token ya expiró, removerlo de la blacklist
                _blacklistedTokens.TryRemove(tokenHash, out _);
                _logger.LogInformation($"✅ Token expirado removido de blacklist");
                return false;
            }
        }

        // Token no está en blacklist
        return false;
    }

    /// <summary>
    /// Limpia tokens que ya expiraron de la blacklist
    /// Debería ejecutarse periódicamente (por ejemplo, cada hora)
    /// </summary>
    public void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = _blacklistedTokens
            .Where(kvp => kvp.Value <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var tokenHash in expiredTokens)
        {
            if (_blacklistedTokens.TryRemove(tokenHash, out _))
            {
                _logger.LogDebug($"🧹 Token expirado removido de blacklist durante limpieza");
            }
        }

        if (expiredTokens.Count > 0)
        {
            _logger.LogInformation($"🧹 Limpieza de blacklist: {expiredTokens.Count} tokens removidos");
        }
    }

    /// <summary>
    /// Genera un hash consistente del token para almacenamiento seguro
    /// </summary>
    private static string HashToken(string token)
    {
        // Usar solo los últimos 50 caracteres del token para crear el hash
        // Esto evita guardar el token completo y es más eficiente
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
            return System.Convert.ToBase64String(hashedBytes);
        }
    }
}
