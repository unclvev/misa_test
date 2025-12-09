using System;
using System.Collections.Generic;

namespace AssetManagement.Domain.Entities;

public partial class AssetIncreaseVoucher
{
    public ulong Id { get; set; }

    public string VoucherNo { get; set; } = null!;

    public DateOnly VoucherDate { get; set; }

    public DateOnly IncreaseDate { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AssetIncreaseVoucherDetail> Details { get; set; } = new List<AssetIncreaseVoucherDetail>();

    /// <summary>
    /// Tổng nguyên giá (tính toán, không lưu trong database)
    /// Được tính từ SQL query để tối ưu hiệu suất
    /// </summary>
    public decimal TotalOriginalPrice { get; set; }

    /// <summary>
    /// Số lượng details (tính toán, không lưu trong database)
    /// Được tính từ SQL query để tối ưu hiệu suất
    /// </summary>
    public int DetailCount { get; set; }
}

