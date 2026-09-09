namespace AuthService.Msv.DTOs
{
    public class UserDto
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = string.Empty;  

    }
}
