using AssetManagement.Application.Dtos;
using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using System.Linq;

namespace AssetManagement.Application.Services;

public class AssetService : IAssetService
{
    private readonly IAssetRepository _assetRepository;

    private const string AssetCodePrefix = "TS";
    private const int AssetCodeNumberLength = 5;

    public AssetService(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    private async Task<string> GenerateAssetCode()
    {
        // Lấy mã tài sản lớn nhất có prefix TS
        var maxCode = await _assetRepository.GetMaxAssetCodeByPrefix(AssetCodePrefix);
        
        int nextNumber = 1;
        
        if (!string.IsNullOrEmpty(maxCode) && maxCode.StartsWith(AssetCodePrefix))
        {
            // Lấy phần số từ mã hiện tại (bỏ prefix)
            var numberPart = maxCode.Substring(AssetCodePrefix.Length);
            if (int.TryParse(numberPart, out var currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }
        
        // Format số với độ dài cố định (5 chữ số)
        var numberString = nextNumber.ToString().PadLeft(AssetCodeNumberLength, '0');
        
        return $"{AssetCodePrefix}{numberString}";
    }

    public async Task<string> GetNextAssetCode()
    {
        return await GenerateAssetCode();
    }

    public async Task<PagedResultDto<AssetListDto>> GetAllAssetsPaged(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var (assets, totalCount) = await _assetRepository.GetAllPaged(pageNumber, pageSize);

        return MapToAssetListDto(assets, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResultDto<AssetListDto>> FilterAssets(int pageNumber, int pageSize, string? searchText = null, string? deptCode = null, byte? typeCode = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var (assets, totalCount) = await _assetRepository.Filter(pageNumber, pageSize, searchText, deptCode, typeCode);

        return MapToAssetListDto(assets, totalCount, pageNumber, pageSize);
    }

    public async Task<AssetDto?> GetAssetById(ulong id)
    {
        var asset = await _assetRepository.GetById(id);
        if (asset == null)
        {
            return null;
        }
        return MapToDto(asset);
    }

    private static PagedResultDto<AssetListDto> MapToAssetListDto(IEnumerable<Asset> assets, int totalCount, int pageNumber, int pageSize)
    {
        var currentYear = DateTime.Now.Year;
        var assetListDtos = assets.Select(asset =>
        {
            var yearsUsed = asset.StartTrackingYear.HasValue 
                ? Math.Max(0, currentYear - asset.StartTrackingYear.Value) 
                : 0;
            
            var accumulatedDepreciation = asset.AnnualDepreciationValue.HasValue
                ? asset.AnnualDepreciationValue.Value * yearsUsed
                : 0;
            
            var remainingValue = asset.PurchasePrice - accumulatedDepreciation;

            return new AssetListDto
            {
                Id = asset.Id,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                TypeName = asset.Type?.TypeName ?? string.Empty,
                DeptName = asset.Department?.DepartmentName ?? string.Empty,
                Quantity = asset.Quantity,
                PurchasePrice = asset.PurchasePrice,
                AccumulatedDepreciation = accumulatedDepreciation,
                RemainingValue = remainingValue
            };
        }).ToList();

        return new PagedResultDto<AssetListDto>
        {
            Data = assetListDtos,
            TotalRecords = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<AssetDto> CreateAsset(RequestAssetDto createAssetDto)
    {
        // Tự động sinh mã tài sản nếu không được cung cấp
        string assetCode;
        if (string.IsNullOrWhiteSpace(createAssetDto.AssetCode))
        {
            assetCode = await GenerateAssetCode();
        }
        else
        {
            assetCode = createAssetDto.AssetCode.Trim();
            // Validate mã tài sản không trùng
            if (await _assetRepository.ExistsAssetCode(assetCode))
            {
                throw new InvalidOperationException($"Mã tài sản '{assetCode}' đã tồn tại trong hệ thống");
            }
        }

        // Đảm bảo assetCode không rỗng
        if (string.IsNullOrWhiteSpace(assetCode))
        {
            throw new InvalidOperationException("Không thể tạo mã tài sản. Vui lòng thử lại.");
        }

        // Validate mã bộ phận sử dụng
        var department = await _assetRepository.GetDepartmentByCode(createAssetDto.DeptCode);
        if (department == null)
        {
            throw new InvalidOperationException($"Mã bộ phận sử dụng '{createAssetDto.DeptCode}' không tồn tại");
        }

        // Validate mã loại tài sản
        var assetType = await _assetRepository.GetAssetTypeByCode(createAssetDto.TypeCode);
        if (assetType == null)
        {
            throw new InvalidOperationException($"Mã loại tài sản '{createAssetDto.TypeCode}' không tồn tại");
        }

        // Tự động tính các giá trị
        var purchaseYear = (short)createAssetDto.PurchaseDate.Year;
        var startTrackingYear = purchaseYear;
        var yearsOfUse = assetType.YearsOfUse;
        var depreciationRate = assetType.DepreciationRate;
        var annualDepreciationValue = createAssetDto.PurchasePrice * depreciationRate / 100;

        // Tạo entity
        var asset = new Asset
        {
            AssetCode = assetCode,
            AssetSymbol = assetCode, // Sử dụng AssetCode làm AssetSymbol để tránh duplicate (vì AssetCode là unique)
            AssetName = createAssetDto.AssetName,
            DepartmentId = department.Id,
            TypeId = assetType.Id,
            PurchaseDate = createAssetDto.PurchaseDate,
            PurchaseYear = purchaseYear,
            StartTrackingYear = startTrackingYear,
            YearsOfUse = yearsOfUse,
            DepreciationRate = depreciationRate,
            Quantity = createAssetDto.Quantity,
            PurchasePrice = createAssetDto.PurchasePrice,
            AnnualDepreciationValue = annualDepreciationValue,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Lưu vào database
        var createdAsset = await _assetRepository.Add(asset);

        // Load lại với navigation properties
        var assetWithNav = await _assetRepository.GetById(createdAsset.Id);
        if (assetWithNav == null)
        {
            throw new InvalidOperationException("Không thể tải lại thông tin tài sản sau khi tạo");
        }

        // Map sang DTO
        return MapToDto(assetWithNav);
    }

    public async Task<AssetDto> UpdateAsset(ulong id, RequestAssetDto updateAssetDto)
    {
        var existingAsset = await _assetRepository.GetById(id);
        if (existingAsset == null)
        {
            throw new InvalidOperationException($"Không tìm thấy tài sản với ID {id}");
        }

        // Validate mã tài sản không trùng (trừ chính nó)
        if (existingAsset.AssetCode != updateAssetDto.AssetCode && 
            await _assetRepository.ExistsAssetCode(updateAssetDto.AssetCode))
        {
            throw new InvalidOperationException($"Mã tài sản '{updateAssetDto.AssetCode}' đã tồn tại trong hệ thống");
        }

        // Validate mã bộ phận sử dụng
        var department = await _assetRepository.GetDepartmentByCode(updateAssetDto.DeptCode);
        if (department == null)
        {
            throw new InvalidOperationException($"Mã bộ phận sử dụng '{updateAssetDto.DeptCode}' không tồn tại");
        }

        // Validate mã loại tài sản
        var assetType = await _assetRepository.GetAssetTypeByCode(updateAssetDto.TypeCode);
        if (assetType == null)
        {
            throw new InvalidOperationException($"Mã loại tài sản '{updateAssetDto.TypeCode}' không tồn tại");
        }

        // Tự động tính các giá trị
        var purchaseYear = (short)updateAssetDto.PurchaseDate.Year;
        var startTrackingYear = purchaseYear;
        var yearsOfUse = assetType.YearsOfUse;
        var depreciationRate = assetType.DepreciationRate;
        var annualDepreciationValue = updateAssetDto.PurchasePrice * depreciationRate / 100;

        // Cập nhật entity
        existingAsset.AssetCode = updateAssetDto.AssetCode;
        existingAsset.AssetSymbol = updateAssetDto.AssetCode; // Đồng bộ AssetSymbol với AssetCode
        existingAsset.AssetName = updateAssetDto.AssetName;
        existingAsset.DepartmentId = department.Id;
        existingAsset.TypeId = assetType.Id;
        existingAsset.PurchaseDate = updateAssetDto.PurchaseDate;
        existingAsset.PurchaseYear = purchaseYear;
        existingAsset.StartTrackingYear = startTrackingYear;
        existingAsset.YearsOfUse = yearsOfUse;
        existingAsset.DepreciationRate = depreciationRate;
        existingAsset.Quantity = updateAssetDto.Quantity;
        existingAsset.PurchasePrice = updateAssetDto.PurchasePrice;
        existingAsset.AnnualDepreciationValue = annualDepreciationValue;
        existingAsset.UpdatedAt = DateTime.Now;

        var updatedAsset = await _assetRepository.Update(existingAsset);
        var assetWithNav = await _assetRepository.GetById(updatedAsset.Id);
        if (assetWithNav == null)
        {
            throw new InvalidOperationException("Không thể tải lại thông tin tài sản sau khi cập nhật");
        }

        return MapToDto(assetWithNav);
    }

    public async Task<bool> DeleteAsset(ulong id)
    {
        var existingAsset = await _assetRepository.GetById(id);
        if (existingAsset == null)
        {
            throw new InvalidOperationException($"Không tìm thấy tài sản với ID {id}");
        }

        // Kiểm tra xem tài sản có trong chứng từ ghi tăng không
        if (await _assetRepository.IsAssetInIncreaseVoucher(id))
        {
            throw new InvalidOperationException(
                $"Không thể xóa tài sản '{existingAsset.AssetCode} - {existingAsset.AssetName}' vì tài sản này đã có trong chứng từ ghi tăng. " +
                "Vui lòng xóa chứng từ ghi tăng trước khi xóa tài sản.");
        }

        return await _assetRepository.Delete(id);
    }

    public async Task<bool> IsAssetCodeExists(string assetCode)
    {
        return await _assetRepository.ExistsAssetCode(assetCode);
    }

    private static AssetDto MapToDto(Asset asset)
    {
        return new AssetDto
        {
            Id = asset.Id,
            AssetCode = asset.AssetCode,
            AssetName = asset.AssetName,
            DeptCode = asset.Department?.DepartmentCode ?? string.Empty,
            DeptName = asset.Department?.DepartmentName ?? string.Empty,
            TypeCode = asset.Type?.TypeCode ?? 0,
            TypeName = asset.Type?.TypeName ?? string.Empty,
            PurchaseDate = asset.PurchaseDate,
            PurchaseYear = asset.PurchaseYear,
            StartTrackingYear = asset.StartTrackingYear,
            YearsOfUse = asset.YearsOfUse,
            DepreciationRate = asset.DepreciationRate,
            Quantity = asset.Quantity,
            PurchasePrice = asset.PurchasePrice,
            AnnualDepreciationValue = asset.AnnualDepreciationValue,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
    }

    public async Task<int> DeleteManyAsync(IEnumerable<ulong> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return 0;

        // Repository sẽ tự động check và chỉ xóa những asset không có trong chứng từ
        return await _assetRepository.DeleteAssetsByIds(idList);
    }

    public async Task<IEnumerable<DepartmentDto>> GetAllDepartments()
    {
        var departments = await _assetRepository.GetAllDepartments();
        return departments.Select(d => new DepartmentDto
        {
            Id = d.Id,
            DepartmentCode = d.DepartmentCode,
            DepartmentSymbol = d.DepartmentSymbol,
            DepartmentName = d.DepartmentName
        });
    }

    public async Task<IEnumerable<AssetTypeDto>> GetAllAssetTypes()
    {
        var assetTypes = await _assetRepository.GetAllAssetTypes();
        return assetTypes.Select(t => new AssetTypeDto
        {
            Id = t.Id,
            TypeCode = t.TypeCode,
            TypeSymbol = t.TypeSymbol,
            TypeName = t.TypeName,
            YearsOfUse = t.YearsOfUse,
            DepreciationRate = t.DepreciationRate
        });
    }

    public async Task<PagedResultDto<AssetListDto>> FilterAvailableForVoucher(int pageNumber, int pageSize, string? searchText = null, string? deptCode = null, byte? typeCode = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var (assets, totalCount) = await _assetRepository.FilterAvailableForVoucher(pageNumber, pageSize, searchText, deptCode, typeCode);

        return MapToAssetListDto(assets, totalCount, pageNumber, pageSize);
    }
}

