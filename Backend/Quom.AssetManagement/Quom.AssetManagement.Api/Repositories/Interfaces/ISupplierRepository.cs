using Quom.AssetManagement.Api.DTOs.Suppliers;
using Quom.AssetManagement.Api.Models;
using System.Text.Json;

namespace Quom.AssetManagement.Api.Repositories.Interfaces
{
    public interface ISupplierRepository
    {
        Task<IEnumerable<Supplier>> GetAllAsync();
        Task<int> CreateAsync(CreateSupplierRequest request);
        Task<IEnumerable<ServiceType>> GetServicesAsync(int supplierId);

        Task SetServicesAsync(
            int supplierId,
            SetSupplierServicesRequest request);
    }
}