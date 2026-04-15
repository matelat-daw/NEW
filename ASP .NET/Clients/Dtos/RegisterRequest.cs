namespace Clients.Dtos;

public class RegisterRequest
{
    public string Nick { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Surname1 { get; set; } = null!;
    public string? Surname2 { get; set; }
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? Gender { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? ProfilePicture { get; set; }
}
