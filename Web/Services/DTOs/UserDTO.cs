namespace Web.Services.DTOs;

public class UserDTO
{
    public string? Name { get; set; }
    public string? Nickname { get; set; }
    public string? Picture { get; set; }
}

public class UserUpdateDTO
{
    public string? Picture { get; set; }
    public string? Name { get; set; }
    public string? Nickname { get; set; }

}