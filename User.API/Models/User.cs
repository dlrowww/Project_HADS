namespace User.API.Models;

public class User_repository
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;  // 实际项目应加密
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

