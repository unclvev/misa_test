using AssetManagement.Application.Dtos;

namespace AssetManagement.Application.Interfaces;

public interface IIncreaseVoucherService
{
    Task<PagedResultDto<IncreaseVoucherListDto>> GetAllVouchersPaged(int pageNumber, int pageSize);
    Task<PagedResultDto<IncreaseVoucherListDto>> FilterVouchers(int pageNumber, int pageSize, string? searchText = null, DateOnly? fromDate = null, DateOnly? toDate = null, decimal? totalOriginalPrice = null);
    Task<IncreaseVoucherDto?> GetVoucherById(ulong id);
    Task<IncreaseVoucherDto> CreateVoucher(RequestIncreaseVoucherDto createVoucherDto);
    Task<IncreaseVoucherDto> UpdateVoucher(ulong id, RequestIncreaseVoucherDto updateVoucherDto);
    Task<bool> DeleteVoucher(ulong id);
    Task<bool> DeleteVoucherDetail(ulong detailId);
    Task<int> DeleteManyAsync(IEnumerable<ulong> ids);
    Task<bool> IsVoucherNoExists(string voucherNo);
    Task<string> GetNextVoucherNo();
}

