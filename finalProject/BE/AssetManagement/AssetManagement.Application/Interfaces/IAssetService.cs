using AssetManagement.Application.Dtos;

namespace AssetManagement.Application.Interfaces;

public interface IAssetService
{
    Task<PagedResultDto<AssetListDto>> GetAllAssetsPaged(int pageNumber, int pageSize);
    Task<PagedResultDto<AssetListDto>> FilterAssets(int pageNumber, int pageSize, string? searchText = null, string? deptCode = null, byte? typeCode = null);
    Task<AssetDto?> GetAssetById(ulong id);
    Task<AssetDto> CreateAsset(RequestAssetDto createAssetDto);
    Task<AssetDto> UpdateAsset(ulong id, RequestAssetDto updateAssetDto);
    Task<bool> DeleteAsset(ulong id);
    Task<bool> IsAssetCodeExists(string assetCode);
    Task<int> DeleteManyAsync(IEnumerable<ulong> ids);
    Task<IEnumerable<DepartmentDto>> GetAllDepartments();
    Task<IEnumerable<AssetTypeDto>> GetAllAssetTypes();
    Task<string> GetNextAssetCode();
    Task<PagedResultDto<AssetListDto>> FilterAvailableForVoucher(int pageNumber, int pageSize, string? searchText = null, string? deptCode = null, byte? typeCode = null);
}

