using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Quom.AssetManagement.Api.DTOs.Auth;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;
using Quom.AssetManagement.Api.Security;
using Quom.AssetManagement.Api.Services.Implementations;

namespace Quom.AssetManagement.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IPasswordHasher<UserAccount>> _passwordHasherMock;
        private readonly JwtTokenService _jwtTokenService;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _passwordHasherMock = new Mock<IPasswordHasher<UserAccount>>();

            var configurationData = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "Test-Key-For-JWT-Unit-Tests-1234567890",
                ["Jwt:Issuer"] = "Quom.Tests",
                ["Jwt:Audience"] = "Quom.Tests.Client",
                ["Jwt:ExpirationMinutes"] = "60"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();

            _jwtTokenService = new JwtTokenService(configuration);

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _passwordHasherMock.Object,
                _jwtTokenService);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenUserDoesNotExist()
        {
            // Arrange
            var request = new LoginRequest
            {
                Login = "no-existe",
                Password = "Password123!"
            };

            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByLoginAsync(request.Login))
                .ReturnsAsync((UserAccount?)null);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);

            _passwordHasherMock.Verify(
                hasher => hasher.VerifyHashedPassword(
                    It.IsAny<UserAccount>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenUserIsInactive()
        {
            // Arrange
            var user = CreateValidUser();
            user.IsActive = false;

            var request = CreateValidLoginRequest();

            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByLoginAsync(request.Login))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);

            _passwordHasherMock.Verify(
                hasher => hasher.VerifyHashedPassword(
                    It.IsAny<UserAccount>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenUserIsLockedOut()
        {
            // Arrange
            var user = CreateValidUser();
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);

            var request = CreateValidLoginRequest();

            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByLoginAsync(request.Login))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);

            _passwordHasherMock.Verify(
                hasher => hasher.VerifyHashedPassword(
                    It.IsAny<UserAccount>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_RegistersFailedAttempt_WhenPasswordIsInvalid()
        {
            // Arrange
            var user = CreateValidUser();
            var request = CreateValidLoginRequest();

            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByLoginAsync(request.Login))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password))
                .Returns(PasswordVerificationResult.Failed);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.Null(result);

            _userRepositoryMock.Verify(
                repository =>
                    repository.RegisterFailedLoginAsync(user.Id),
                Times.Once);

            _userRepositoryMock.Verify(
                repository =>
                    repository.ResetLoginAttemptsAsync(
                        It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
        {
            // Arrange
            var user = CreateValidUser();
            var request = CreateValidLoginRequest();

            _userRepositoryMock
                .Setup(repository =>
                    repository.GetByLoginAsync(request.Login))
                .ReturnsAsync(user);

            _passwordHasherMock
                .Setup(hasher => hasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    request.Password))
                .Returns(PasswordVerificationResult.Success);

            _userRepositoryMock
                .Setup(repository =>
                    repository.ResetLoginAttemptsAsync(user.Id))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.LoginAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(string.IsNullOrWhiteSpace(result.Token));
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.Username, result.Username);
            Assert.Equal(user.RoleName, result.Role);
            Assert.True(result.ExpiresAt > DateTime.UtcNow);

            _userRepositoryMock.Verify(
                repository =>
                    repository.ResetLoginAttemptsAsync(user.Id),
                Times.Once);

            _userRepositoryMock.Verify(
                repository =>
                    repository.RegisterFailedLoginAsync(
                        It.IsAny<int>()),
                Times.Never);
        }

        private static UserAccount CreateValidUser()
        {
            return new UserAccount
            {
                Id = 1,
                Username = "admin",
                Email = "admin@empresa.com",
                PasswordHash = "HASH_DE_PRUEBA",
                RoleId = 1,
                RoleName = "Administrador",
                IsActive = true,
                FailedLoginAttempts = 0,
                LockoutEnd = null
            };
        }

        private static LoginRequest CreateValidLoginRequest()
        {
            return new LoginRequest
            {
                Login = "admin",
                Password = "Admin123!"
            };
        }
    }
}