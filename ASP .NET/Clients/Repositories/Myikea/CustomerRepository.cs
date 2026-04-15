using Clients.Entities.Myikea;
using Clients.Data;
using Microsoft.EntityFrameworkCore;

namespace Clients.Repositories.Myikea
{
    /// <summary>
    /// Repositorio para la entidad Customer (MyIkea)
    /// </summary>
    public interface ICustomerRepository
    {
        /// <summary>
        /// Obtiene un customer por su ID
        /// </summary>
        Task<Customer?> GetByIdAsync(long customerId);

        /// <summary>
        /// Obtiene un customer por email
        /// </summary>
        Task<Customer?> GetByEmailAsync(string email);

        /// <summary>
        /// Verifica si existe un customer con el email especificado
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Obtiene todos los customers
        /// </summary>
        Task<List<Customer>> GetAllAsync();

        /// <summary>
        /// Busca customers por nombre (búsqueda parcial, case-insensitive)
        /// </summary>
        Task<List<Customer>> SearchByFirstNameAsync(string firstName);

        /// <summary>
        /// Busca customers por apellido (búsqueda parcial, case-insensitive)
        /// </summary>
        Task<List<Customer>> SearchByLastNameAsync(string lastName);

        /// <summary>
        /// Busca customers por nombre o apellido
        /// </summary>
        Task<List<Customer>> SearchByNameAsync(string firstName, string lastName);

        /// <summary>
        /// Obtiene customers con paginación
        /// </summary>
        Task<(List<Customer> items, long totalCount)> GetPagedAsync(int page, int pageSize);

        /// <summary>
        /// Obtiene el total de customers
        /// </summary>
        Task<long> GetCountAsync();

        /// <summary>
        /// Crea un nuevo customer
        /// </summary>
        Task<Customer> AddAsync(Customer customer);

        /// <summary>
        /// Actualiza un customer existente
        /// </summary>
        Task<Customer> UpdateAsync(Customer customer);

        /// <summary>
        /// Elimina un customer por su ID
        /// </summary>
        Task DeleteAsync(long customerId);

        /// <summary>
        /// Verifica si existe un customer con el ID especificado
        /// </summary>
        Task<bool> ExistsByIdAsync(long customerId);
    }

    /// <summary>
    /// Implementación del repositorio para Customer
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly MyikeaDbContext _context;
        private readonly ILogger<CustomerRepository> _logger;

        public CustomerRepository(MyikeaDbContext context, ILogger<CustomerRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Customer?> GetByIdAsync(long customerId)
        {
            try
            {
                return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener customer por ID: {ex.Message}");
                return null;
            }
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener customer por email: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            try
            {
                return await _context.Customers.AnyAsync(c => c.Email == email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al verificar existencia de email: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            try
            {
                return await _context.Customers.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener todos los customers: {ex.Message}");
                return new List<Customer>();
            }
        }

        public async Task<List<Customer>> SearchByFirstNameAsync(string firstName)
        {
            try
            {
                return await _context.Customers
                    .Where(c => c.FirstName.ToLower().Contains(firstName.ToLower()))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar customers por nombre: {ex.Message}");
                return new List<Customer>();
            }
        }

        public async Task<List<Customer>> SearchByLastNameAsync(string lastName)
        {
            try
            {
                return await _context.Customers
                    .Where(c => c.LastName.ToLower().Contains(lastName.ToLower()))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar customers por apellido: {ex.Message}");
                return new List<Customer>();
            }
        }

        public async Task<List<Customer>> SearchByNameAsync(string firstName, string lastName)
        {
            try
            {
                return await _context.Customers
                    .Where(c => c.FirstName.ToLower().Contains(firstName.ToLower()) ||
                                c.LastName.ToLower().Contains(lastName.ToLower()))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar customers: {ex.Message}");
                return new List<Customer>();
            }
        }

        public async Task<(List<Customer> items, long totalCount)> GetPagedAsync(int page, int pageSize)
        {
            try
            {
                var totalCount = await _context.Customers.CountAsync();
                var items = await _context.Customers
                    .Skip(page * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener customers paginados: {ex.Message}");
                return (new List<Customer>(), 0);
            }
        }

        public async Task<long> GetCountAsync()
        {
            try
            {
                return await _context.Customers.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al contar customers: {ex.Message}");
                return 0;
            }
        }

        public async Task<Customer> AddAsync(Customer customer)
        {
            try
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al agregar customer: {ex.Message}");
                throw;
            }
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            try
            {
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar customer: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(long customerId)
        {
            try
            {
                var customer = await GetByIdAsync(customerId);
                if (customer == null)
                {
                    throw new InvalidOperationException($"Customer no encontrado con ID: {customerId}");
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar customer: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ExistsByIdAsync(long customerId)
        {
            try
            {
                return await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al verificar existencia de customer: {ex.Message}");
                return false;
            }
        }
    }
}
