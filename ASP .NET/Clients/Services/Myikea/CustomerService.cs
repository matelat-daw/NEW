using Clients.Entities.Myikea;
using Clients.Repositories.Myikea;

namespace Clients.Services.Myikea
{
    /// <summary>
    /// Interfaz del servicio para gestionar operaciones de Customer
    /// </summary>
    public interface ICustomerService
    {
        Task<Customer?> GetCustomerByIdAsync(long customerId);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<List<Customer>> GetAllCustomersAsync();
        Task<List<Customer>> SearchByFirstNameAsync(string firstName);
        Task<List<Customer>> SearchByLastNameAsync(string lastName);
        Task<List<Customer>> SearchByNameAsync(string firstName, string lastName);
        Task<bool> CustomerExistsByEmailAsync(string email);
        Task<(List<Customer> items, long totalCount)> GetPagedCustomersAsync(int page, int pageSize);
        Task<long> GetTotalCustomersAsync();
        Task<Customer> CreateCustomerAsync(Customer customer);
        Task<Customer> UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(long customerId);
    }

    /// <summary>
    /// Servicio para gestionar operaciones de Customer (MyIkea)
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un customer por su ID
        /// </summary>
        public async Task<Customer?> GetCustomerByIdAsync(long customerId)
        {
            try
            {
                return await _customerRepository.GetByIdAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener customer por ID: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene un customer por su email
        /// </summary>
        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            try
            {
                return await _customerRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener customer por email: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene todos los customers
        /// </summary>
        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            try
            {
                return await _customerRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener todos los customers: {ex.Message}");
                return new List<Customer>();
            }
        }

        /// <summary>
        /// Obtiene customers por nombre (búsqueda parcial)
        /// </summary>
        public async Task<List<Customer>> SearchByFirstNameAsync(string firstName)
        {
            try
            {
                return await _customerRepository.SearchByFirstNameAsync(firstName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar customers por nombre: {ex.Message}");
                return new List<Customer>();
            }
        }

        /// <summary>
        /// Obtiene customers por apellido (búsqueda parcial)
        /// </summary>
        public async Task<List<Customer>> SearchByLastNameAsync(string lastName)
        {
            try
            {
                return await _customerRepository.SearchByLastNameAsync(lastName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar customers por apellido: {ex.Message}");
                return new List<Customer>();
            }
        }

        /// <summary>
        /// Busca customers por nombre o apellido
        /// </summary>
        public async Task<List<Customer>> SearchByNameAsync(string firstName, string lastName)
        {
            try
            {
                return await _customerRepository.SearchByNameAsync(firstName, lastName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar customers: {ex.Message}");
                return new List<Customer>();
            }
        }

        /// <summary>
        /// Verifica si existe un customer con el email
        /// </summary>
        public async Task<bool> CustomerExistsByEmailAsync(string email)
        {
            try
            {
                return await _customerRepository.ExistsByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al verificar existencia de email: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene customers con paginación
        /// </summary>
        public async Task<(List<Customer> items, long totalCount)> GetPagedCustomersAsync(int page, int pageSize)
        {
            try
            {
                return await _customerRepository.GetPagedAsync(page, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener customers paginados: {ex.Message}");
                return (new List<Customer>(), 0);
            }
        }

        /// <summary>
        /// Obtiene el total de customers
        /// </summary>
        public async Task<long> GetTotalCustomersAsync()
        {
            try
            {
                return await _customerRepository.GetCountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener total de customers: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Crea un nuevo customer
        /// </summary>
        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            try
            {
                if (await _customerRepository.ExistsByEmailAsync(customer.Email ?? ""))
                {
                    throw new InvalidOperationException($"Ya existe un customer con el email: {customer.Email}");
                }

                return await _customerRepository.AddAsync(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Actualiza un customer existente
        /// </summary>
        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                if (!await _customerRepository.ExistsByIdAsync(customer.CustomerId))
                {
                    throw new InvalidOperationException($"Customer no encontrado con ID: {customer.CustomerId}");
                }

                return await _customerRepository.UpdateAsync(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Elimina un customer por su ID
        /// </summary>
        public async Task DeleteCustomerAsync(long customerId)
        {
            try
            {
                if (!await _customerRepository.ExistsByIdAsync(customerId))
                {
                    throw new InvalidOperationException($"Customer no encontrado con ID: {customerId}");
                }

                await _customerRepository.DeleteAsync(customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar customer: {ex.Message}");
                throw;
            }
        }
    }
}
