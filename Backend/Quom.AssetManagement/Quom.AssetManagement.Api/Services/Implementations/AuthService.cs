using Microsoft.AspNetCore.Identity;
using Quom.AssetManagement.Api.DTOs.Auth;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;
using Quom.AssetManagement.Api.Services.Interfaces;
using Quom.AssetManagement.Api.Security;


namespace Quom.AssetManagement.Api.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<UserAccount> _passwordHasher;
        private readonly JwtTokenService _jwtTokenService;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher<UserAccount> passwordHasher,
            JwtTokenService jwtTokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByLoginAsync(request.Login);

            if (user is null || !user.IsActive)
                return null;

            if (user.LockoutEnd.HasValue &&
                user.LockoutEnd.Value > DateTime.UtcNow)
            {
                return null;
            }

            var verification = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (verification == PasswordVerificationResult.Failed)
            {
                await _userRepository.RegisterFailedLoginAsync(user.Id);
                return null;
            }

            await _userRepository.ResetLoginAttemptsAsync(user.Id);

            var tokenResult = _jwtTokenService.GenerateToken(user);

            return new LoginResponse
            {
                Token = tokenResult.Token,
                ExpiresAt = tokenResult.ExpiresAt,
                UserId = user.Id,
                Username = user.Username,
                Role = user.RoleName
            };
        }

    }
}