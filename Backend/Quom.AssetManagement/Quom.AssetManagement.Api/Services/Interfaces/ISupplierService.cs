using Quom.AssetManagement.Api.DTOs.Suppliers;
using Quom.AssetManagement.Api.Models;

namespace Quom.AssetManagement.Api.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<int> CreateAsync(CreateSupplierRequest request);
        Task<IEnumerable<ServiceType>> GetServicesAsync(int supplierId);

        Task SetServicesAsync(
            int supplierId,
            SetSupplierServicesRequest request);
    }
}