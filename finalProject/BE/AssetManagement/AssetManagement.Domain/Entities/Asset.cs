using System;
using System.Collections.Generic;

namespace AssetManagement.Domain.Entities;

public partial class Asset
{
    public ulong Id { get; set; }

    public string AssetCode { get; set; } = null!;

    public string AssetSymbol { get; set; } = null!;

    public string AssetName { get; set; } = null!;

    public uint DepartmentId { get; set; }

    public uint TypeId { get; set; }

    public DateOnly PurchaseDate { get; set; }

    public short? PurchaseYear { get; set; }

    public short? StartTrackingYear { get; set; }

    public uint YearsOfUse { get; set; }

    public decimal DepreciationRate { get; set; }

    public uint Quantity { get; set; }

    public decimal PurchasePrice { get; set; }

    public decimal? AnnualDepreciationValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual AssetType Type { get; set; } = null!;

    public virtual ICollection<AssetIncreaseVoucherDetail> IncreaseVoucherDetails { get; set; } = new List<AssetIncreaseVoucherDetail>();
}
