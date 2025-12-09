using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces;

public interface IAssetRepository
{
    Task<IEnumerable<Asset>> GetAll();
    Task<(IEnumerable<Asset> Assets, int TotalCount)> GetAllPaged(int pageNumber, int pageSize);
    Task<(IEnumerable<Asset> Assets, int TotalCount)> Filter(int pageNumber, int pageSize, string? searchText = null, string? deptCode = null, byte? typeCode = null);
    Task<Asset?> GetById(ulong id);
    Task<Asset?> GetByAssetCode(string assetCode);
    Task<Asset> Add(Asset asset);
    Task<Asset> Update(Asset asset);
    Task<bool> Delete(ulong id);
    Task<bool> ExistsAssetCode(string assetCode);
    Task<string?> GetMaxAssetCodeByPrefix(string prefix);
    Task<Department?> GetDepartmentByCode(string deptCode);
    Task<AssetType?> GetAssetTypeByCode(byte typeCode);
    Task<IEnumerable<Department>> GetAllDepartments();
    Task<IEnumerable<AssetType>> GetAllAssetTypes();
    Task<int> DeleteAssetsByIds(IEnumerable<ulong> ids);
    Task<bool> IsAssetInIncreaseVoucher(ulong assetId);
    Task<(IEnumerable<Asset> Assets, int TotalCount)> FilterAvailableForVoucher(int pageNumber, int pageSize, string? searchText = null, string? deptCode = null, byte? typeCode = null);
}

