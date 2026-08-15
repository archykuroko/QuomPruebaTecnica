using Quom.AssetManagement.Api.Models;

namespace Quom.AssetManagement.Api.Repositories.Interfaces
{
    public interface IServiceTypeRepository
    {
        Task<IEnumerable<ServiceType>> GetAllAsync();
    }
}