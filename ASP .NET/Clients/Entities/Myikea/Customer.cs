using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Clients.Entities.Myikea
{
    /// <summary>
    /// Entidad Customer para la base de datos myikea
    /// Mapea la tabla customer
    /// </summary>
    [Table("customer")]
    public class Customer
    {
        [Key]
        [Column("customer_id")]
        public long CustomerId { get; set; }

        [Column("first_name")]
        [Required]
        [StringLength(45)]
        public string FirstName { get; set; } = string.Empty;

        [Column("last_name")]
        [Required]
        [StringLength(45)]
        public string LastName { get; set; } = string.Empty;

        [Column("telefono")]
        [StringLength(9)]
        public string? Telefono { get; set; }

        [Column("email")]
        [StringLength(50)]
        public string? Email { get; set; }

        [Column("fecha_de_nacimiento")]
        public DateTime? FechaDeNacimiento { get; set; }
    }
}
