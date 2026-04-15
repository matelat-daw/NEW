using System.Text.Json.Serialization;
using Clients.Entities.Myikea;

namespace Clients.Dtos.Myikea
{
    /// <summary>
    /// DTO para Customer (MyIkea)
    /// </summary>
    public class CustomerDto
    {
        [JsonPropertyName("customerId")]
        public long CustomerId { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("telefono")]
        public string? Telefono { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("fechaDeNacimiento")]
        public DateTime? FechaDeNacimiento { get; set; }

        /// <summary>
        /// Convierte una entidad Customer a CustomerDto
        /// </summary>
        public static CustomerDto FromEntity(Customer customer)
        {
            return new CustomerDto
            {
                CustomerId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Telefono = customer.Telefono,
                Email = customer.Email,
                FechaDeNacimiento = customer.FechaDeNacimiento
            };
        }

        /// <summary>
        /// Convierte un CustomerDto a entidad Customer
        /// </summary>
        public Customer ToEntity()
        {
            return new Customer
            {
                CustomerId = CustomerId,
                FirstName = FirstName,
                LastName = LastName,
                Telefono = Telefono,
                Email = Email,
                FechaDeNacimiento = FechaDeNacimiento
            };
        }
    }
}
