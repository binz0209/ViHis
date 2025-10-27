using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using VietHistory.Domain.Entities;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Services;

namespace VietHistory.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMongoContext _context;
    private readonly JwtService _jwtService;

    public AuthController(IMongoContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    /// <summary>Đăng ký tài khoản mới</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Kiểm tra username đã tồn tại
            var existingUser = await _context.Users
                .Find(u => u.Username == request.Username || u.Email == request.Email)
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                return BadRequest(new { error = "Username hoặc email đã tồn tại" });
            }

            // Tạo user mới
            var user = new AppUser
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.InsertOneAsync(user);

            // Tạo JWT token
            var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email);

            return Ok(new AuthResponse
            {
                Token = token,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Đăng nhập</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Tìm user
            var user = await _context.Users
                .Find(u => u.Username == request.Username)
                .FirstOrDefaultAsync();

            if (user == null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { error = "Username hoặc password không đúng" });
            }

            // Tạo JWT token
            var token = _jwtService.GenerateToken(user.Id, user.Username, user.Email);

            // Update last login time
            user.UpdatedAt = DateTime.UtcNow;
            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            return Ok(new AuthResponse
            {
                Token = token,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Lấy thông tin user hiện tại</summary>
    [HttpGet("me")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult<UserInfo>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>Đổi password</summary>
    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
                         ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var user = await _context.Users
                .Find(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            // Verify old password
            if (!PasswordHasher.VerifyPassword(request.OldPassword, user.PasswordHash))
            {
                return BadRequest(new { error = "Mật khẩu cũ không đúng" });
            }

            // Update password
            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);

            return Ok(new { message = "Đổi password thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserInfo User { get; set; } = null!;
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

