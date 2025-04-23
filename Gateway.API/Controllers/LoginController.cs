using Microsoft.AspNetCore.Mvc;
using OfferInventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;    // AppDbContext
using OfferInventory.Domain.Entities;          // User
using System.Threading.Tasks;
using System;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoginController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        
        var users = await _context.Users.ToListAsync();
        Console.WriteLine($"数据库中用户数: {users.Count}");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

        if (user != null)
        {
            return Ok(new { token = "real-token-or-id" });
        }

        return Unauthorized();
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    

    
}
