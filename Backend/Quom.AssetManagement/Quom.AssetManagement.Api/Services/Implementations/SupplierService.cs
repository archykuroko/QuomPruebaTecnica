using Quom.AssetManagement.Api.DTOs.Suppliers;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;
using Quom.AssetManagement.Api.Services.Interfaces;

namespace Quom.AssetManagement.Api.Services.Implementations
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierService(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        public Task<IEnumerable<Supplier>> GetAllAsync()
        {
            return _supplierRepository.GetAllAsync();
        }

        public Task<int> CreateAsync(CreateSupplierRequest request)
        {
            return _supplierRepository.CreateAsync(request);
        }

        public Task<IEnumerable<ServiceType>> GetServicesAsync(int supplierId)
        {
            if (supplierId <= 0)
                throw new ArgumentException(
                    "El identificador del proveedor no es válido.");

            return _supplierRepository.GetServicesAsync(supplierId);
        }

        public Task SetServicesAsync(
            int supplierId,
            SetSupplierServicesRequest request)
        {
            if (supplierId <= 0)
                throw new ArgumentException(
                    "El identificador del proveedor no es válido.");

            if (request.ServiceTypeIds.Count == 0)
                throw new ArgumentException(
                    "Debe seleccionarse al menos un tipo de servicio.");

            return _supplierRepository.SetServicesAsync(
                supplierId,
                request);
        }

    }
}