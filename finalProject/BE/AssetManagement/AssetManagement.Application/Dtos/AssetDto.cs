namespace AssetManagement.Application.Dtos;

public class AssetDto
{
    public ulong Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string DeptCode { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public byte TypeCode { get; set; }
    public string TypeName { get; set; } = string.Empty;
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
}

