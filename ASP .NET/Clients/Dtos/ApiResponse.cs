using System.Text.Json.Serialization;

namespace Clients.Dtos;

/// <summary>
/// Respuesta genérica unificada para toda la API
/// Implementa un formato consistente: success, message, data, statusCode, timestamp
/// Equivalente a ApiResponse.java en la API JAVA
/// </summary>
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ApiResponse<T> Ok(string message, T? data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crea una respuesta exitosa con solo mensaje
    /// </summary>
    public static ApiResponse<T> Ok(string message)
    {
        return Ok(message, default);
    }

    /// <summary>
    /// Crea una respuesta de error con status code
    /// </summary>
    public static ApiResponse<T> Error(int statusCode, string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = default,
            StatusCode = statusCode,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Crea una respuesta de error 400
    /// </summary>
    public static ApiResponse<T> BadRequest(string message)
    {
        return Error(400, message);
    }

    /// <summary>
    /// Crea una respuesta de error 401
    /// </summary>
    public static ApiResponse<T> Unauthorized(string message)
    {
        return Error(401, message);
    }

    /// <summary>
    /// Crea una respuesta de error 403
    /// </summary>
    public static ApiResponse<T> Forbidden(string message)
    {
        return Error(403, message);
    }

    /// <summary>
    /// Crea una respuesta de error 404
    /// </summary>
    public static ApiResponse<T> NotFound(string message)
    {
        return Error(404, message);
    }

    /// <summary>
    /// Crea una respuesta de error 500
    /// </summary>
    public static ApiResponse<T> InternalServerError(string message)
    {
        return Error(500, message);
    }
}
