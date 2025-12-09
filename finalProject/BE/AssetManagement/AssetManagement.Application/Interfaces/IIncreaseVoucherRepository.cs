using AssetManagement.Domain.Entities;

namespace AssetManagement.Application.Interfaces;

public interface IIncreaseVoucherRepository
{
    Task<(IEnumerable<AssetIncreaseVoucher> Vouchers, int TotalCount)> GetAllPaged(int pageNumber, int pageSize);
    Task<(IEnumerable<AssetIncreaseVoucher> Vouchers, int TotalCount)> Filter(int pageNumber, int pageSize, string? searchText = null, DateOnly? fromDate = null, DateOnly? toDate = null, decimal? totalOriginalPrice = null);
    Task<AssetIncreaseVoucher?> GetById(ulong id);
    Task<AssetIncreaseVoucher?> GetByVoucherNo(string voucherNo);
    Task<AssetIncreaseVoucher> Add(AssetIncreaseVoucher voucher);
    Task<AssetIncreaseVoucher> Update(AssetIncreaseVoucher voucher);
    Task<bool> Delete(ulong id);
    Task<bool> ExistsVoucherNo(string voucherNo);
    Task<string?> GetMaxVoucherNoByPrefix(string prefix);
    Task<IEnumerable<AssetIncreaseVoucherDetail>> GetDetailsByVoucherId(ulong voucherId);
    Task<int> DeleteDetailsByVoucherId(ulong voucherId);
    Task<bool> DeleteDetail(ulong detailId);
    Task<AssetIncreaseVoucherDetail> AddDetail(AssetIncreaseVoucherDetail detail);
    Task<int> DeleteVouchersByIds(IEnumerable<ulong> ids);
}

