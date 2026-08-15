using Quom.AssetManagement.Api.Models;

namespace Quom.AssetManagement.Api.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<UserAccount?> GetByLoginAsync(string login);
        Task RegisterFailedLoginAsync(int userId);
        Task ResetLoginAttemptsAsync(int userId);
    }
}