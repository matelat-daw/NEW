using System.Text.Json.Serialization;
using Clients.Entities;
using Clients.Enums;

namespace Clients.Dtos;

public class UserDto
{
    public long Id { get; set; }
    public string Nick { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Surname1 { get; set; } = null!;
    public string? Surname2 { get; set; }
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    [JsonPropertyName("profileImg")]
    public string? ProfilePicture { get; set; }

    /// <summary>
    /// Rol principal del usuario - serializado como "role" en JSON para compatibilidad con frontend
    /// Se serializa el nombre del enum (ej: "ADMIN") en lugar del valor numérico (ej: 3)
    /// </summary>
    [JsonPropertyName("role")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RoleEnum PrimaryRole { get; set; }

    [JsonIgnore]
    public ICollection<RoleEnum> Roles { get; set; } = new List<RoleEnum>();
    public int Active { get; set; }
    public int EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public static UserDto FromEntity(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Nick = user.Nick,
            Name = user.Name,
            Surname1 = user.Surname1,
            Surname2 = user.Surname2,
            Email = user.Email,
            Phone = user.Phone,
            Gender = user.Gender,
            BirthDate = user.BirthDate,
            ProfilePicture = ConvertToImageUrl(user.ProfilePicture),
            PrimaryRole = user.PrimaryRole,
            Roles = user.Roles,
            Active = user.Active,
            EmailVerified = user.EmailVerified,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    /// <summary>
    /// Convierte una ruta de imagen a URL accesible
    /// Retorna la ruta relativa (sin /uploads/) para que el frontend pueda construir la URL
    /// El frontend usará: API_BASE_URL + /images/ + profileImg
    /// </summary>
    private static string? ConvertToImageUrl(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return null;

        // Si tiene "/uploads/" al inicio, removerlo
        if (imagePath.StartsWith("/uploads/"))
            return imagePath.Substring("/uploads/".Length);

        // Si ya empieza con "uploads/" (sin barra inicial), removerlo
        if (imagePath.StartsWith("uploads/"))
            return imagePath.Substring("uploads/".Length);

        // Si es una ruta relativa limpia, devolverla tal cual
        return imagePath;
    }
}
