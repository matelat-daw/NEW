using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Clients.Dtos.Myikea;
using Clients.Services.Myikea;
using Clients.Entities.Myikea;
using Clients.Helpers;

namespace Clients.Controllers.Myikea
{
    /// <summary>
    /// Controlador para gestión de customers de MyIkea
    /// 
    /// Permisos:
    /// - ADMIN: Acceso total (crear, leer, actualizar, eliminar)
    /// - PREMIUM: Solo lectura (GET endpoints)
    /// - USER: Sin acceso
    /// </summary>
    [ApiController]
    [Route("api/myikea/customer")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene el total de customers registrados (requiere ADMIN o PREMIUM)
        /// NOTA: Este endpoint debe estar ANTES de /{id} para evitar conflictos de rutas
        /// </summary>
        [HttpGet("stats/total")]
        public async Task<IActionResult> GetTotalCustomers()
        {
            try
            {
                // Validar autenticación - cualquier usuario autenticado puede ver
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning($"Acceso denegado a stats/total: usuario no autenticado");
                    return Unauthorized();
                }

                var total = await _customerService.GetTotalCustomersAsync();
                return Ok(new
                {
                    success = true,
                    message = "Total de customers obtenido",
                    total = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener total: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener total: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obtiene un customer por su email (requiere ADMIN o PREMIUM)
        /// NOTA: Este endpoint debe estar ANTES de /{id} para evitar conflictos de rutas
        /// </summary>
        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetCustomerByEmail([FromRoute] string email)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning($"Acceso denegado a by-email/{email}: usuario no autenticado");
                    return Unauthorized();
                }

                var customer = await _customerService.GetCustomerByEmailAsync(email);
                if (customer == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Customer no encontrado con email: {email}"
                    });
                }

                var customerDto = CustomerDto.FromEntity(customer);
                return Ok(new
                {
                    success = true,
                    message = "Customer obtenido exitosamente",
                    customer = customerDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener customer: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener customer: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Busca customers por nombre (requiere ADMIN o PREMIUM)
        /// NOTA: Este endpoint debe estar ANTES de /{id} para evitar conflictos de rutas
        /// </summary>
        [HttpGet("search/firstName/{firstName}")]
        public async Task<IActionResult> SearchByFirstName([FromRoute] string firstName)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning($"Acceso denegado a search/firstName: usuario no autenticado");
                    return Unauthorized();
                }

                var customers = await _customerService.SearchByFirstNameAsync(firstName);
                var customerDtos = customers.Select(CustomerDto.FromEntity).ToList();

                return Ok(new
                {
                    success = true,
                    message = "Búsqueda completada exitosamente",
                    customers = customerDtos,
                    totalResults = customerDtos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar customers: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar customers: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Busca customers por apellido (requiere ADMIN o PREMIUM)
        /// NOTA: Este endpoint debe estar ANTES de /{id} para evitar conflictos de rutas
        /// </summary>
        [HttpGet("search/lastName/{lastName}")]
        public async Task<IActionResult> SearchByLastName([FromRoute] string lastName)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning($"Acceso denegado a search/lastName: usuario no autenticado");
                    return Unauthorized();
                }

                var customers = await _customerService.SearchByLastNameAsync(lastName);
                var customerDtos = customers.Select(CustomerDto.FromEntity).ToList();

                return Ok(new
                {
                    success = true,
                    message = "Búsqueda completada exitosamente",
                    customers = customerDtos,
                    totalResults = customerDtos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al buscar customers: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al buscar customers: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obtiene la lista de todos los customers (requiere ADMIN o PREMIUM)
        /// Soporta paginación
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers([FromQuery] int page = 0, [FromQuery] int size = 10)
        {
            try
            {
                // Validar autenticación - cualquier usuario autenticado puede ver
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning($"Acceso denegado a GET /api/myikea/customer: usuario no autenticado");
                    return Unauthorized();
                }

                var (customers, totalCount) = await _customerService.GetPagedCustomersAsync(page, size);
                var customerDtos = customers.Select(CustomerDto.FromEntity).ToList();

                var totalPages = (int)Math.Ceiling((double)totalCount / size);

                return Ok(new
                {
                    success = true,
                    message = "Lista de customers obtenida exitosamente",
                    customers = customerDtos,
                    pagination = new
                    {
                        currentPage = page,
                        totalItems = totalCount,
                        totalPages = totalPages,
                        pageSize = size,
                        hasNext = page < totalPages - 1,
                        hasPrevious = page > 0
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener lista de customers: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener lista de customers: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Obtiene un customer específico por su ID (requiere ADMIN o PREMIUM)
        /// NOTA: Este endpoint debe estar DESPUÉS de las rutas específicas (stats, by-email, search)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById([FromRoute] long id)
        {
            try
            {
                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning($"Acceso denegado a GET /{id}: usuario no autenticado");
                    return Unauthorized();
                }

                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Customer no encontrado con ID: {id}"
                    });
                }

                var customerDto = CustomerDto.FromEntity(customer);
                return Ok(new
                {
                    success = true,
                    message = "Customer obtenido exitosamente",
                    customer = customerDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al obtener customer: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al obtener customer: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Elimina un customer por su ID (requiere ADMIN solamente)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer([FromRoute] long id)
        {
            try
            {
                if (!AuthorizationHelper.IsAdmin(User))
                {
                    _logger.LogWarning($"Acceso denegado a DELETE /{id}: usuario no es ADMIN");
                    return Forbid();
                }

                await _customerService.DeleteCustomerAsync(id);
                return Ok(new
                {
                    success = true,
                    message = "Customer eliminado exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al eliminar customer: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al eliminar customer: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Actualiza un customer (requiere ADMIN solamente)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer([FromRoute] long id, [FromBody] CustomerDto customerDto)
        {
            try
            {
                if (!AuthorizationHelper.IsAdmin(User))
                {
                    _logger.LogWarning($"Acceso denegado a PUT /{id}: usuario no es ADMIN");
                    return Forbid();
                }

                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = $"Customer no encontrado con ID: {id}"
                    });
                }

                // Actualizar campos si se proporcionaron
                if (!string.IsNullOrEmpty(customerDto.FirstName))
                    customer.FirstName = customerDto.FirstName;

                if (!string.IsNullOrEmpty(customerDto.LastName))
                    customer.LastName = customerDto.LastName;

                if (!string.IsNullOrEmpty(customerDto.Telefono))
                    customer.Telefono = customerDto.Telefono;

                if (!string.IsNullOrEmpty(customerDto.Email))
                    customer.Email = customerDto.Email;

                if (customerDto.FechaDeNacimiento.HasValue)
                    customer.FechaDeNacimiento = customerDto.FechaDeNacimiento;

                var updatedCustomer = await _customerService.UpdateCustomerAsync(customer);
                var updatedDto = CustomerDto.FromEntity(updatedCustomer);

                return Ok(new
                {
                    success = true,
                    message = "Customer actualizado exitosamente",
                    customer = updatedDto
                });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al actualizar customer: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al actualizar customer: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Crea un nuevo customer (requiere ADMIN solamente)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerDto customerDto)
        {
            try
            {
                if (!AuthorizationHelper.IsAdmin(User))
                {
                    _logger.LogWarning($"Acceso denegado a POST: usuario no es ADMIN");
                    return Forbid();
                }

                if (string.IsNullOrEmpty(customerDto.FirstName) || string.IsNullOrEmpty(customerDto.LastName))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "FirstName y LastName son requeridos"
                    });
                }

                var customer = new Customer
                {
                    FirstName = customerDto.FirstName,
                    LastName = customerDto.LastName,
                    Telefono = customerDto.Telefono,
                    Email = customerDto.Email,
                    FechaDeNacimiento = customerDto.FechaDeNacimiento
                };

                var createdCustomer = await _customerService.CreateCustomerAsync(customer);
                var createdDto = CustomerDto.FromEntity(createdCustomer);

                return CreatedAtAction(nameof(GetCustomerById), new { id = createdCustomer.CustomerId }, new
                {
                    success = true,
                    message = "Customer creado exitosamente",
                    customer = createdDto
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al crear customer: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error al crear customer: {ex.Message}"
                });
            }
        }
    }
}
