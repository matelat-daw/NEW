using Microsoft.EntityFrameworkCore;
using Clients.Entities.Myikea;

namespace Clients.Data
{
    /// <summary>
    /// DbContext para la base de datos secundaria myikea
    /// </summary>
    public class MyikeaDbContext : DbContext
    {
        public MyikeaDbContext(DbContextOptions<MyikeaDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la tabla customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customer");
                entity.HasKey(e => e.CustomerId);
                
                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(45)
                    .IsRequired();

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(45)
                    .IsRequired();

                entity.Property(e => e.Telefono)
                    .HasColumnName("telefono")
                    .HasMaxLength(9);

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(50);

                entity.Property(e => e.FechaDeNacimiento)
                    .HasColumnName("fecha_de_nacimiento");

                // Crear índice único en email
                entity.HasIndex(e => e.Email).IsUnique();
            });
        }
    }
}
