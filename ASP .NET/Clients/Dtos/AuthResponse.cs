namespace Clients.Dtos;

public class AuthResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = null!;
    public string? Token { get; set; }
    public UserDto? User { get; set; }
    public object? Data { get; set; }

    public static AuthResponse Success(string message, string? token = null, UserDto? user = null)
    {
        return new AuthResponse
        {
            IsSuccess = true,
            Message = message,
            Token = token,
            User = user,
            Data = null
        };
    }

    public static AuthResponse Error(string message)
    {
        return new AuthResponse
        {
            IsSuccess = false,
            Message = message,
            Token = null,
            User = null,
            Data = null
        };
    }

    public static AuthResponseBuilder Builder()
    {
        return new AuthResponseBuilder();
    }
}

public class AuthResponseBuilder
{
    private bool _isSuccess;
    private string _message = null!;
    private string? _token;
    private UserDto? _user;
    private object? _data;

    public AuthResponseBuilder SetSuccess(bool success)
    {
        _isSuccess = success;
        return this;
    }

    public AuthResponseBuilder SetMessage(string message)
    {
        _message = message;
        return this;
    }

    public AuthResponseBuilder SetToken(string? token)
    {
        _token = token;
        return this;
    }

    public AuthResponseBuilder SetUser(UserDto? user)
    {
        _user = user;
        return this;
    }

    public AuthResponseBuilder SetData(object? data)
    {
        _data = data;
        return this;
    }

    public AuthResponse Build()
    {
        return new AuthResponse
        {
            IsSuccess = _isSuccess,
            Message = _message,
            Token = _token,
            User = _user,
            Data = _data
        };
    }
}
