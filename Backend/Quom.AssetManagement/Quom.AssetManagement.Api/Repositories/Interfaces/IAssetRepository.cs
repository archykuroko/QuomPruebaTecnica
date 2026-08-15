using Quom.AssetManagement.Api.DTOs;
using Quom.AssetManagement.Api.DTOs.Assets;
using Quom.AssetManagement.Api.Models;


namespace Quom.AssetManagement.Api.Repositories.Interfaces
{
    public interface IAssetRepository
    {
        Task<Asset?> GetByIdAsync(int id);
        Task<PagedResult<Asset>> SearchAsync(AssetSearchRequest request);
        Task<int> CreateAsync(
            CreateAssetRequest request,
            int performedByUserId);

        Task UpdateAsync(
            int id,
            UpdateAssetRequest request,
            int performedByUserId);

        Task AssignAsync(
            int assetId,
            AssignAssetRequest request,
            int performedByUserId);

        Task ReturnAsync(
            int assetId,
            ReturnAssetRequest request,
            int performedByUserId);
        Task<IEnumerable<AssetMovement>> GetHistoryAsync(int assetId);
    }
} 