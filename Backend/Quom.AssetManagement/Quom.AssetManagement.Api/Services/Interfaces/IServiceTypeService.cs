using Quom.AssetManagement.Api.Models;

namespace Quom.AssetManagement.Api.Services.Interfaces
{
    public interface IServiceTypeService
    {
        Task<IEnumerable<ServiceType>> GetAllAsync();
    }
}