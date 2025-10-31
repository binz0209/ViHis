using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using VietHistory.Api.Controllers;
using VietHistory.Domain.Entities;
using VietHistory.Infrastructure.Mongo;
using VietHistory.Infrastructure.Services;
using Xunit;

namespace VietHistory.AI.Tests
{
    [Trait("Feature", "AUTH_JWT")]
    [Trait("Integration", "Real")]
    public class AUTH_JWT_IntegrationTests
    {
        private readonly IMongoContext _mongo;
        private readonly JwtService _jwt;
        private readonly AuthController _controller;

        public AUTH_JWT_IntegrationTests()
        {
            var mongoSettings = new MongoSettings
            {
                ConnectionString = "mongodb+srv://lanserveUser:Binzdapoet.020904@hlinhwfil.eunq7.mongodb.net/?retryWrites=true&w=majority&appName=HlinhWfil",
                Database = "vihis_auth_test"
            };
            _mongo = new MongoContext(mongoSettings);

            var jwtOpt = new JwtOptions
            {
                Key = "test-secret-key-12345678901234567890",
                Issuer = "VietHistory.Api",
                Audience = "VietHistory.Client",
                ExpirationMinutes = 5
            };
            _jwt = new JwtService(jwtOpt);
            _controller = new AuthController(_mongo, _jwt)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC01_Register_Duplicate_Should_Return_400()
        {
            var username = $"dup_{Guid.NewGuid():N}";
            _ = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "abc12345" });
            var resp = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "abc12345" });
            resp.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC02_Register_Then_Login_Should_Succeed()
        {
            var username = $"user_{Guid.NewGuid():N}";
            var register = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "P@ssw0rd!" });
            register.Result.Should().BeOfType<OkObjectResult>();

            var login = await _controller.Login(new LoginRequest { Username = username, Password = "P@ssw0rd!" });
            var auth = (login.Result as OkObjectResult)?.Value as AuthResponse ?? login.Value;
            auth!.Token.Should().NotBeNullOrEmpty();
            auth.User.Username.Should().Be(username);
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P0")]
        public async Task TC03_Login_WrongPassword_Should_Return_401()
        {
            var username = $"user_{Guid.NewGuid():N}";
            _ = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "correct" });
            var resp = await _controller.Login(new LoginRequest { Username = username, Password = "wrong" });
            resp.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P1")]
        public async Task TC04_Login_WrongUsername_Should_Return_401()
        {
            var resp = await _controller.Login(new LoginRequest { Username = "no_user_"+Guid.NewGuid().ToString("N"), Password = "whatever" });
            resp.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        [Trait("Category", "Me")]
        [Trait("Priority", "P1")]
        public async Task TC05_Me_MissingToken_Should_Return_401()
        {
            var me = await _controller.GetCurrentUser();
            me.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        [Trait("Category", "Me")]
        [Trait("Priority", "P1")]
        public async Task TC06_Me_With_ValidClaims_Should_Return_Profile()
        {
            var username = $"u_{Guid.NewGuid():N}";
            var regResp = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "abc12345" });
            var loginResp = await _controller.Login(new LoginRequest { Username = username, Password = "abc12345" });
            var login = (loginResp.Result as OkObjectResult)?.Value as AuthResponse ?? loginResp.Value!;
            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, login.User.Id),
                    new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, login.User.Id),
                }, "TestAuth"));
            _controller.HttpContext.User = principal;
            var meResp = await _controller.GetCurrentUser();
            var me = (meResp.Result as OkObjectResult)?.Value as UserInfo ?? meResp.Value!;
            me!.Username.Should().Be(username);
        }

        [Fact]
        [Trait("Category", "ChangePassword")]
        [Trait("Priority", "P0")]
        public async Task TC07_ChangePassword_HappyPath_Then_Login_With_New()
        {
            var username = $"c_{Guid.NewGuid():N}";
            var regResp = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "oldpass1" });
            var loginResp = await _controller.Login(new LoginRequest { Username = username, Password = "oldpass1" });
            var login = (loginResp.Result as OkObjectResult)?.Value as AuthResponse ?? loginResp.Value!;
            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, login.User.Id),
                    new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, login.User.Id),
                }, "TestAuth"));
            _controller.HttpContext.User = principal;
            _ = await _controller.ChangePassword(new ChangePasswordRequest { OldPassword = "oldpass1", NewPassword = "newpass1" });
            var login2Resp = await _controller.Login(new LoginRequest { Username = username, Password = "newpass1" });
            var login2 = (login2Resp.Result as OkObjectResult)?.Value as AuthResponse ?? login2Resp.Value!;
            login2.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "ChangePassword")]
        [Trait("Priority", "P1")]
        public async Task TC08_ChangePassword_WrongOld_Should_Return_400()
        {
            var username = $"d_{Guid.NewGuid():N}";
            var regResp = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "oldpass1" });
            var loginResp = await _controller.Login(new LoginRequest { Username = username, Password = "oldpass1" });
            var login = (loginResp.Result as OkObjectResult)?.Value as AuthResponse ?? loginResp.Value!;
            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, login.User.Id),
                    new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, login.User.Id),
                }, "TestAuth"));
            _controller.HttpContext.User = principal;
            var resp = await _controller.ChangePassword(new ChangePasswordRequest { OldPassword = "WRONG", NewPassword = "newpass1" });
            resp.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        [Trait("Category", "Performance")]
        [Trait("Priority", "P2")]
        public async Task TC09_Parallel_Login_Consistency()
        {
            var username = $"p_{Guid.NewGuid():N}";
            _ = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "pppppppp" });
            var tasks = Enumerable.Range(0,5).Select(_ => _controller.Login(new LoginRequest { Username = username, Password = "pppppppp" }));
            var results = await Task.WhenAll(tasks);
            results.All(r => (r.Result as OkObjectResult) != null || r.Value != null).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public async Task TC10_Register_DuplicateEmail_Should_Return_400()
        {
            var email = $"e_{Guid.NewGuid():N}@mail.com";
            var u1 = $"u1_{Guid.NewGuid():N}";
            var u2 = $"u2_{Guid.NewGuid():N}";
            _ = await _controller.Register(new RegisterRequest { Username = u1, Email = email, Password = "abc12345" });
            var resp = await _controller.Register(new RegisterRequest { Username = u2, Email = email, Password = "abc12345" });
            resp.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        [Trait("Category", "ErrorHandling")]
        [Trait("Priority", "P1")]
        public async Task TC11_Login_EmptyFields_Should_Return_401_CurrentBehavior()
        {
            var resp = await _controller.Login(new LoginRequest { Username = string.Empty, Password = string.Empty });
            resp.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        [Trait("Category", "Me")]
        [Trait("Priority", "P1")]
        public async Task TC12_Me_MissingClaims_Should_Return_401()
        {
            _controller.HttpContext.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
            var me = await _controller.GetCurrentUser();
            me.Result.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        [Trait("Category", "Me")]
        [Trait("Priority", "P1")]
        public async Task TC13_Me_DeletedUser_Should_Return_404()
        {
            var username = $"del_{Guid.NewGuid():N}";
            var reg = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "abc12345" });
            var auth = (reg.Result as OkObjectResult)?.Value as AuthResponse ?? throw new Exception("register failed");
            var userId = auth.User.Id;
            await (_mongo.Users as IMongoCollection<AppUser>)!.DeleteOneAsync(u => u.Id == userId);
            var principal = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(new[]
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId),
                    new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, userId),
                }, "TestAuth"));
            _controller.HttpContext.User = principal;
            var meResp = await _controller.GetCurrentUser();
            meResp.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        [Trait("Category", "ChangePassword")]
        [Trait("Priority", "P1")]
        public async Task TC14_ChangePassword_MissingToken_Should_Return_404_CurrentBehavior()
        {
            var resp = await _controller.ChangePassword(new ChangePasswordRequest { OldPassword = "x", NewPassword = "y" });
            resp.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P2")]
        public async Task TC15_Register_UnicodeUsername_Should_Succeed()
        {
            var username = $"NgườiDùng_{Guid.NewGuid():N}";
            var resp = await _controller.Register(new RegisterRequest { Username = username, Email = $"{Guid.NewGuid():N}@mail.com", Password = "abc12345" });
            resp.Result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        [Trait("Category", "EdgeCase")]
        [Trait("Priority", "P2")]
        public async Task TC16_Login_Whitespace_Around_Username_Should_Return_401_CurrentBehavior()
        {
            var username = $"ws_{Guid.NewGuid():N}";
            _ = await _controller.Register(new RegisterRequest { Username = username, Email = username+"@mail.com", Password = "abc12345" });
            var resp = await _controller.Login(new LoginRequest { Username = " " + username + " ", Password = "abc12345" });
            resp.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}


