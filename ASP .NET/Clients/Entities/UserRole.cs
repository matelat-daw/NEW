using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clients.Entities;

[Table("user_roles")]
public class UserRole
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    public long UserId { get; set; }

    [Required]
    [Column("role_id")]
    public int RoleId { get; set; }

    [Required]
    [Column("assigned_at")]
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("RoleId")]
    public Role? Role { get; set; }
}
