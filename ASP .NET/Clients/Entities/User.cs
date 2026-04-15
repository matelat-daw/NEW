using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Clients.Enums;

namespace Clients.Entities;

[Table("users")]
public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("nick")]
    [StringLength(255)]
    public string Nick { get; set; } = null!;

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [Column("surname1")]
    [StringLength(255)]
    public string Surname1 { get; set; } = null!;

    [Column("surname2")]
    [StringLength(255)]
    public string? Surname2 { get; set; }

    [Required]
    [Column("email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    [Column("phone")]
    [StringLength(255)]
    public string Phone { get; set; } = null!;

    [Required]
    [Column("password")]
    [StringLength(255)]
    public string Password { get; set; } = null!;

    [Column("gender")]
    public string? Gender { get; set; }

    [Column("bday")]
    public DateTime? BirthDate { get; set; }

    [Column("profile_img")]
    [StringLength(255)]
    public string? ProfilePicture { get; set; }

    [Required]
    [Column("active")]
    public int Active { get; set; } = 0;

    [Required]
    [Column("email_verified")]
    public int EmailVerified { get; set; } = 0;

    [Column("verification_token")]
    [StringLength(255)]
    public string? VerificationToken { get; set; }

    [Column("verification_token_expiry")]
    public DateTime? VerificationTokenExpiry { get; set; }

    [Required]
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation property for many-to-many relationship
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // Helper property to get all roles for this user
    [NotMapped]
    public ICollection<RoleEnum> Roles
    {
        get => UserRoles.Where(ur => ur.Role != null)
            .Select(ur => (RoleEnum)ur.RoleId)
            .ToList();
    }

    // Helper property to get the primary role (first assigned role)
    [NotMapped]
    public RoleEnum PrimaryRole
    {
        get
        {
            var firstRole = UserRoles.OrderBy(ur => ur.AssignedAt).FirstOrDefault();
            return firstRole != null ? (RoleEnum)firstRole.RoleId : RoleEnum.USER;
        }
    }

    // Helper method to check if user has a specific role
    public bool HasRole(RoleEnum role)
    {
        return UserRoles.Any(ur => ur.RoleId == (int)role);
    }

    // Helper method to check if user is admin
    public bool IsAdmin => HasRole(RoleEnum.ADMIN);

    // Helper method to check if user is premium or admin
    public bool IsPremiumOrAdmin => HasRole(RoleEnum.PREMIUM) || HasRole(RoleEnum.ADMIN);
}
