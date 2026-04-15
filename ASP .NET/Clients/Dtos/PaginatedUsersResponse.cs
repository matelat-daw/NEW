namespace Clients.Dtos;

public class PaginatedUsersResponse
{
    public List<UserDto> Content { get; set; } = new();
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public bool Last { get; set; }
    public bool First { get; set; }
}
