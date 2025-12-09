using AssetManagement.Application.Dtos;
using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using System.Linq;

namespace AssetManagement.Application.Services;

public class IncreaseVoucherService : IIncreaseVoucherService
{
    private readonly IIncreaseVoucherRepository _voucherRepository;
    private readonly IAssetRepository _assetRepository;

    private const string VoucherNoPrefix = "GT";
    private const int VoucherNoNumberLength = 5;

    public IncreaseVoucherService(
        IIncreaseVoucherRepository voucherRepository,
        IAssetRepository assetRepository)
    {
        _voucherRepository = voucherRepository;
        _assetRepository = assetRepository;
    }

    private async Task<string> GenerateVoucherNo()
    {
        // Lấy mã chứng từ lớn nhất có prefix GT
        var maxCode = await _voucherRepository.GetMaxVoucherNoByPrefix(VoucherNoPrefix);
        
        int nextNumber = 1;
        
        if (!string.IsNullOrEmpty(maxCode) && maxCode.StartsWith(VoucherNoPrefix))
        {
            // Lấy phần số từ mã hiện tại (bỏ prefix)
            var numberPart = maxCode.Substring(VoucherNoPrefix.Length);
            if (int.TryParse(numberPart, out var currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }
        
        // Format số với độ dài cố định (5 chữ số)
        var numberString = nextNumber.ToString().PadLeft(VoucherNoNumberLength, '0');
        
        return $"{VoucherNoPrefix}{numberString}";
    }

    public async Task<string> GetNextVoucherNo()
    {
        return await GenerateVoucherNo();
    }

    public async Task<PagedResultDto<IncreaseVoucherListDto>> GetAllVouchersPaged(int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var (vouchers, totalCount) = await _voucherRepository.GetAllPaged(pageNumber, pageSize);

        return MapToVoucherListDto(vouchers, totalCount, pageNumber, pageSize);
    }

    public async Task<PagedResultDto<IncreaseVoucherListDto>> FilterVouchers(int pageNumber, int pageSize, string? searchText = null, DateOnly? fromDate = null, DateOnly? toDate = null, decimal? totalOriginalPrice = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var (vouchers, totalCount) = await _voucherRepository.Filter(pageNumber, pageSize, searchText, fromDate, toDate, totalOriginalPrice);

        return MapToVoucherListDto(vouchers, totalCount, pageNumber, pageSize);
    }

    public async Task<IncreaseVoucherDto?> GetVoucherById(ulong id)
    {
        var voucher = await _voucherRepository.GetById(id);
        if (voucher == null)
        {
            return null;
        }
        return MapToDto(voucher);
    }

    public async Task<IncreaseVoucherDto> CreateVoucher(RequestIncreaseVoucherDto createVoucherDto)
    {
        // Tự động sinh mã chứng từ nếu không được cung cấp
        string voucherNo;
        if (string.IsNullOrWhiteSpace(createVoucherDto.VoucherNo))
        {
            voucherNo = await GenerateVoucherNo();
        }
        else
        {
            voucherNo = createVoucherDto.VoucherNo.Trim();
            // Validate mã chứng từ không trùng
            if (await _voucherRepository.ExistsVoucherNo(voucherNo))
            {
                throw new InvalidOperationException($"Số chứng từ '{voucherNo}' đã tồn tại trong hệ thống");
            }
        }

        // Tạo entity voucher
        var voucher = new AssetIncreaseVoucher
        {
            VoucherNo = voucherNo,
            VoucherDate = createVoucherDto.VoucherDate,
            IncreaseDate = createVoucherDto.IncreaseDate,
            Note = createVoucherDto.Note,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Lưu voucher vào database
        var createdVoucher = await _voucherRepository.Add(voucher);

        // Thêm các chi tiết nếu có (cho phép tạo voucher với details rỗng)
        if (createVoucherDto.Details != null && createVoucherDto.Details.Count > 0)
        {
            // Validate từng tài sản có tồn tại không
            var assetIds = createVoucherDto.Details.Select(d => d.AssetId).Distinct().ToList();
            foreach (var assetId in assetIds)
            {
                var asset = await _assetRepository.GetById(assetId);
                if (asset == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy tài sản với ID {assetId}");
                }
            }

            // Thêm các chi tiết
            for (int i = 0; i < createVoucherDto.Details.Count; i++)
            {
                var detailDto = createVoucherDto.Details[i];
                var detail = new AssetIncreaseVoucherDetail
                {
                    VoucherId = createdVoucher.Id,
                    AssetId = detailDto.AssetId,
                    CreatedAt = DateTime.Now
                };
                await _voucherRepository.AddDetail(detail);
            }
        }

        // Load lại với navigation properties
        var voucherWithNav = await _voucherRepository.GetById(createdVoucher.Id);
        if (voucherWithNav == null)
        {
            throw new InvalidOperationException("Không thể tải lại thông tin chứng từ sau khi tạo");
        }

        // Map sang DTO
        return MapToDto(voucherWithNav);
    }

    public async Task<IncreaseVoucherDto> UpdateVoucher(ulong id, RequestIncreaseVoucherDto updateVoucherDto)
    {
        var existingVoucher = await _voucherRepository.GetById(id);
        if (existingVoucher == null)
        {
            throw new InvalidOperationException($"Không tìm thấy chứng từ với ID {id}");
        }

        // Validate mã chứng từ không trùng (trừ chính nó)
        if (existingVoucher.VoucherNo != updateVoucherDto.VoucherNo && 
            await _voucherRepository.ExistsVoucherNo(updateVoucherDto.VoucherNo))
        {
            throw new InvalidOperationException($"Số chứng từ '{updateVoucherDto.VoucherNo}' đã tồn tại trong hệ thống");
        }

        // Validate danh sách tài sản
        if (updateVoucherDto.Details == null || updateVoucherDto.Details.Count == 0)
        {
            throw new InvalidOperationException("Chứng từ phải có ít nhất 1 tài sản");
        }

        // Validate từng tài sản có tồn tại không
        var assetIds = updateVoucherDto.Details.Select(d => d.AssetId).Distinct().ToList();
        foreach (var assetId in assetIds)
        {
            var asset = await _assetRepository.GetById(assetId);
            if (asset == null)
            {
                throw new InvalidOperationException($"Không tìm thấy tài sản với ID {assetId}");
            }
        }

        // Cập nhật thông tin voucher
        existingVoucher.VoucherNo = updateVoucherDto.VoucherNo;
        existingVoucher.VoucherDate = updateVoucherDto.VoucherDate;
        existingVoucher.IncreaseDate = updateVoucherDto.IncreaseDate;
        existingVoucher.Note = updateVoucherDto.Note;
        existingVoucher.UpdatedAt = DateTime.Now;

        // Xóa các chi tiết cũ
        await _voucherRepository.DeleteDetailsByVoucherId(id);

        // Thêm các chi tiết mới
        for (int i = 0; i < updateVoucherDto.Details.Count; i++)
        {
            var detailDto = updateVoucherDto.Details[i];
            var detail = new AssetIncreaseVoucherDetail
            {
                VoucherId = id,
                AssetId = detailDto.AssetId,
            };
            await _voucherRepository.AddDetail(detail);
        }

        // Cập nhật voucher
        var updatedVoucher = await _voucherRepository.Update(existingVoucher);
        var voucherWithNav = await _voucherRepository.GetById(updatedVoucher.Id);
        if (voucherWithNav == null)
        {
            throw new InvalidOperationException("Không thể tải lại thông tin chứng từ sau khi cập nhật");
        }

        return MapToDto(voucherWithNav);
    }

    public async Task<bool> DeleteVoucher(ulong id)
    {
        var existingVoucher = await _voucherRepository.GetById(id);
        if (existingVoucher == null)
        {
            return false;
        }

        return await _voucherRepository.Delete(id);
    }

    public async Task<bool> DeleteVoucherDetail(ulong detailId)
    {
        return await _voucherRepository.DeleteDetail(detailId);
    }

    public async Task<int> DeleteManyAsync(IEnumerable<ulong> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return 0;

        return await _voucherRepository.DeleteVouchersByIds(idList);
    }

    public async Task<bool> IsVoucherNoExists(string voucherNo)
    {
        return await _voucherRepository.ExistsVoucherNo(voucherNo);
    }

    private static PagedResultDto<IncreaseVoucherListDto> MapToVoucherListDto(
        IEnumerable<AssetIncreaseVoucher> vouchers, 
        int totalCount, 
        int pageNumber, 
        int pageSize)
    {
        var voucherListDtos = vouchers.Select(voucher =>
        {
            // TotalOriginalPrice và DetailCount đã được tính trong SQL query, không cần tính lại
            return new IncreaseVoucherListDto
            {
                Id = voucher.Id,
                VoucherNo = voucher.VoucherNo,
                VoucherDate = voucher.VoucherDate,
                IncreaseDate = voucher.IncreaseDate,
                Note = voucher.Note,
                DetailCount = voucher.DetailCount, // Lấy từ entity (đã tính trong SQL)
                TotalOriginalPrice = voucher.TotalOriginalPrice, // Lấy từ entity (đã tính trong SQL)
                CreatedAt = voucher.CreatedAt,
                UpdatedAt = voucher.UpdatedAt
            };
        }).ToList();

        return new PagedResultDto<IncreaseVoucherListDto>
        {
            Data = voucherListDtos,
            TotalRecords = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static IncreaseVoucherDto MapToDto(AssetIncreaseVoucher voucher)
    {
        var details = voucher.Details?.Select(detail => new IncreaseVoucherDetailDto
        {
            Id = detail.Id,
            VoucherId = detail.VoucherId,
            AssetId = detail.AssetId,
            AssetCode = detail.Asset?.AssetCode ?? string.Empty,
            AssetName = detail.Asset?.AssetName ?? string.Empty,
            CreatedAt = detail.CreatedAt
        }).ToList() ?? new List<IncreaseVoucherDetailDto>();

        return new IncreaseVoucherDto
        {
            Id = voucher.Id,
            VoucherNo = voucher.VoucherNo,
            VoucherDate = voucher.VoucherDate,
            IncreaseDate = voucher.IncreaseDate,
            Note = voucher.Note,
            CreatedAt = voucher.CreatedAt,
            UpdatedAt = voucher.UpdatedAt,
            Details = details
        };
    }
}

