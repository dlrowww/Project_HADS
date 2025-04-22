using Microsoft.AspNetCore.Mvc;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // 这里是假登录逻辑，可以换成真正的验证
        if (request.Username == "admin" && request.Password == "1234")
        {
            return Ok(new { token = "fake-jwt-token" });
        }

        return Unauthorized();
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}

