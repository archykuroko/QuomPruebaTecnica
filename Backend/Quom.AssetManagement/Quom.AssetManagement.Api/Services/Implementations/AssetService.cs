using Quom.AssetManagement.Api.DTOs;
using Quom.AssetManagement.Api.DTOs.Assets;
using Quom.AssetManagement.Api.Models;
using Quom.AssetManagement.Api.Repositories.Interfaces;
using Quom.AssetManagement.Api.Services.Interfaces;

namespace Quom.AssetManagement.Api.Services.Implementations
{
    public class AssetService : IAssetService
    {
        private readonly IAssetRepository _assetRepository;

        public AssetService(IAssetRepository assetRepository)
        {
            _assetRepository = assetRepository;
        }

        public async Task<Asset?> GetByIdAsync(int id)
        {
            // Aquí vivirá la lógica de aplicación relacionada con activos
            // El acceso directo a SQL permanece encapsulado en el Repository
            return await _assetRepository.GetByIdAsync(id);
        }

        public async Task<PagedResult<Asset>> SearchAsync(AssetSearchRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Status) &&
            request.Status is not (
                "Disponible" or
                "Asignado" or
                "Mantenimiento" or
                "Retirado"))
            {
                throw new ArgumentException(
                    "El estado del activo no es válido.");
            }

            // Evita solicitudes con valores de paginación inválidos
            if (request.PageNumber < 1)
                request.PageNumber = 1;

            if (request.PageSize < 1)
                request.PageSize = 10;

            if (request.PageSize > 100)
                request.PageSize = 100;

            return await _assetRepository.SearchAsync(request);
        }



        // Para la creación de activos
        public async Task<int> CreateAsync(
             CreateAssetRequest request,
             int performedByUserId)
        {
            if (request.OwnershipType is not ("Propio" or "Arrendado"))
                throw new ArgumentException(
                    "El tipo de propiedad no es válido.");

            if (request.OwnershipType == "Arrendado")
            {
                if (!request.SupplierId.HasValue)
                    throw new ArgumentException(
                        "Un activo arrendado debe tener proveedor.");

                if (!request.RentalEndDate.HasValue)
                    throw new ArgumentException(
                        "Un activo arrendado debe indicar la fecha de término.");
            }

            return await _assetRepository.CreateAsync(
                request,
                performedByUserId);
        }


        // Para la actualización de activos
        public async Task UpdateAsync(
            int id,
            UpdateAssetRequest request,
            int performedByUserId)
        {
            var currentAsset = await _assetRepository.GetByIdAsync(id);

            if (currentAsset is null)
                throw new KeyNotFoundException("El activo no existe.");

            if (currentAsset.Status == "Retirado" &&
                request.Status != "Retirado")
            {
                throw new ArgumentException(
                    "Un activo retirado no puede volver a activarse.");
            }
            if (request.OwnershipType is not ("Propio" or "Arrendado"))
                throw new ArgumentException(
                    "El tipo de propiedad no es válido.");

            if (request.Status == "Asignado")
            {
                throw new ArgumentException(
                    "El estado Asignado solo puede establecerse mediante el proceso de asignación.");
            }

            if (request.Status is not (
                "Disponible" or
                "Mantenimiento" or
                "Retirado"))
            {
                throw new ArgumentException(
                    "El estado del activo no es válido.");
            }

            if (request.OwnershipType == "Arrendado")
            {
                if (!request.SupplierId.HasValue)
                    throw new ArgumentException(
                        "Un activo arrendado debe tener proveedor.");

                if (!request.RentalEndDate.HasValue)
                    throw new ArgumentException(
                        "Un activo arrendado debe indicar la fecha de término.");
            }

            await _assetRepository.UpdateAsync(
                id,
                request,
                performedByUserId);
        }


        // Para asignación de activos
        public async Task AssignAsync(
            int assetId,
            AssignAssetRequest request,
            int performedByUserId)
        {
            if (assetId <= 0)
                throw new ArgumentException(
                    "El identificador del activo no es válido.");

            if (request.EmployeeId <= 0)
                throw new ArgumentException(
                    "El identificador del colaborador no es válido.");

            if (performedByUserId <= 0)
                throw new ArgumentException(
                    "El identificador del usuario no es válido.");

            await _assetRepository.AssignAsync(
                assetId,
                request,
                performedByUserId);
        }

        // Para regresar un activo
        public async Task ReturnAsync(
            int assetId,
            ReturnAssetRequest request,
            int performedByUserId)
        {
            if (assetId <= 0)
                throw new ArgumentException(
                    "El identificador del activo no es válido.");

            if (performedByUserId <= 0)
                throw new ArgumentException(
                    "El identificador del usuario no es válido.");

            if (string.IsNullOrWhiteSpace(request.ReturnCondition))
                throw new ArgumentException(
                    "La condición de devolución es obligatoria.");

            await _assetRepository.ReturnAsync(
                assetId,
                request,
                performedByUserId);
        }

        // Para regresar el historial de activos
        public async Task<IEnumerable<AssetMovement>> GetHistoryAsync(int assetId)
        {
            if (assetId <= 0)
                throw new ArgumentException(
                    "El identificador del activo no es válido.");

            return await _assetRepository.GetHistoryAsync(assetId);
        }


    }
}