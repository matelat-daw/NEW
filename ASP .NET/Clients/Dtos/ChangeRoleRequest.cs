namespace Clients.Dtos;

using Clients.Enums;

public class ChangeRoleRequest
{
    public long UserId { get; set; }
    public RoleEnum NewRole { get; set; }
}
