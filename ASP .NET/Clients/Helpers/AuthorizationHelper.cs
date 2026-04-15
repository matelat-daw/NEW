using System.Security.Claims;

namespace Clients.Helpers
{
    /// <summary>
    /// Helper para validación centralizada de autorización basada en roles
    /// Evita repetición de código en controladores
    /// Similar a Spring Security en Java
    /// </summary>
    public static class AuthorizationHelper
    {
        /// <summary>
        /// Valores de roles como se almacenan en JWT
        /// </summary>
        private const string ROLE_ADMIN = "3";
        private const string ROLE_PREMIUM = "2";
        private const string ROLE_USER = "1";

        /// <summary>
        /// Obtiene el valor numérico del rol del usuario desde los claims
        /// Retorna null si no hay rol
        /// </summary>
        public static string? GetUserRole(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// Verifica si el usuario es ADMIN
        /// </summary>
        public static bool IsAdmin(ClaimsPrincipal user)
        {
            return GetUserRole(user) == ROLE_ADMIN;
        }

        /// <summary>
        /// Verifica si el usuario es PREMIUM
        /// </summary>
        public static bool IsPremium(ClaimsPrincipal user)
        {
            return GetUserRole(user) == ROLE_PREMIUM;
        }

        /// <summary>
        /// Verifica si el usuario es USER (rol básico)
        /// </summary>
        public static bool IsBasicUser(ClaimsPrincipal user)
        {
            return GetUserRole(user) == ROLE_USER;
        }

        /// <summary>
        /// Verifica si el usuario es ADMIN o PREMIUM (puede leer/consultar)
        /// </summary>
        public static bool IsAdminOrPremium(ClaimsPrincipal user)
        {
            var role = GetUserRole(user);
            return role == ROLE_ADMIN || role == ROLE_PREMIUM;
        }

        /// <summary>
        /// Verifica si el usuario es ADMIN (puede modificar/eliminar)
        /// </summary>
        public static bool CanModify(ClaimsPrincipal user)
        {
            return IsAdmin(user);
        }

        /// <summary>
        /// Obtiene el email del usuario desde los claims
        /// </summary>
        public static string? GetUserEmail(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Obtiene el ID del usuario desde los claims
        /// </summary>
        public static string? GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst("userId")?.Value;
        }
    }
}
