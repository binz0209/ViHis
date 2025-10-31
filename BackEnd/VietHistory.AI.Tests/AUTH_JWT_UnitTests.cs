using System;
using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using VietHistory.Infrastructure.Services;
using Xunit;

namespace VietHistory.AI.Tests
{
    [Trait("Feature", "AUTH_JWT")]
    public class AUTH_JWT_UnitTests
    {
        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public void TC01_GenerateToken_Should_Contain_Standard_Claims()
        {
            var jwt = new JwtService(new JwtOptions { Key = "test-secret-key-12345678901234567890", Issuer = "VietHistory.Api", Audience = "VietHistory.Client", ExpirationMinutes = 5 });
            var token = jwt.GenerateToken("uid1", "alice", "alice@mail.com");
            var handler = new JwtSecurityTokenHandler();
            var parsed = handler.ReadJwtToken(token);
            parsed.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "uid1");
            parsed.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.UniqueName && c.Value == "alice");
            parsed.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "alice@mail.com");
        }

        [Fact]
        [Trait("Category", "Security")]
        [Trait("Priority", "P0")]
        public void TC02_ValidateToken_Should_Return_Null_On_Tampered_Token()
        {
            var svc = new JwtService(new JwtOptions { Key = "test-secret-key-12345678901234567890", Issuer = "VietHistory.Api", Audience = "VietHistory.Client", ExpirationMinutes = 5 });
            var token = svc.GenerateToken("uid1", "alice", "alice@mail.com");
            // Tamper: cut last char
            var tampered = token.Substring(0, token.Length - 1) + 'x';
            svc.ValidateToken(tampered).Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Security")]
        [Trait("Priority", "P1")]
        public void TC03_ValidateToken_Should_Return_Null_On_Expired()
        {
            var svc = new JwtService(new JwtOptions { Key = "test-secret-key-12345678901234567890", Issuer = "VietHistory.Api", Audience = "VietHistory.Client", ExpirationMinutes = -1 });
            var token = svc.GenerateToken("uid1", "alice", "alice@mail.com");
            svc.ValidateToken(token).Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Security")]
        [Trait("Priority", "P1")]
        public void TC04_ValidateToken_Should_Return_Null_On_Wrong_Audience()
        {
            var svcA = new JwtService(new JwtOptions { Key = "k12345678901234567890k1234567890", Issuer = "A", Audience = "A", ExpirationMinutes = 5 });
            var token = svcA.GenerateToken("uid", "u", "e@mail");
            var svcB = new JwtService(new JwtOptions { Key = "k12345678901234567890k1234567890", Issuer = "A", Audience = "B", ExpirationMinutes = 5 });
            svcB.ValidateToken(token).Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Security")]
        [Trait("Priority", "P1")]
        public void TC05_ValidateToken_Should_Return_Null_On_Wrong_Issuer()
        {
            var svcA = new JwtService(new JwtOptions { Key = "k12345678901234567890k1234567890", Issuer = "A", Audience = "X", ExpirationMinutes = 5 });
            var token = svcA.GenerateToken("uid", "u", "e@mail");
            var svcB = new JwtService(new JwtOptions { Key = "k12345678901234567890k1234567890", Issuer = "B", Audience = "X", ExpirationMinutes = 5 });
            svcB.ValidateToken(token).Should().BeNull();
        }

        [Fact]
        [Trait("Category", "HappyPath")]
        [Trait("Priority", "P0")]
        public void TC06_ValidateToken_Should_Succeed_Before_Expiry()
        {
            var svc = new JwtService(new JwtOptions { Key = "test-secret-key-12345678901234567890", Issuer = "VietHistory.Api", Audience = "VietHistory.Client", ExpirationMinutes = 1 });
            var token = svc.GenerateToken("uid2", "bob", "bob@mail.com");
            svc.ValidateToken(token).Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "TokenSemantics")]
        [Trait("Priority", "P2")]
        public void TC07_GenerateToken_Should_Contain_Jti()
        {
            var jwt = new JwtService(new JwtOptions { Key = "test-secret-key-12345678901234567890", Issuer = "VietHistory.Api", Audience = "VietHistory.Client", ExpirationMinutes = 5 });
            var token = jwt.GenerateToken("uid3", "charlie", "charlie@mail.com");
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var parsed = handler.ReadJwtToken(token);
            parsed.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti && !string.IsNullOrWhiteSpace(c.Value));
        }
    }
}


