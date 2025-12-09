namespace AssetManagement.Application.Dtos;

public class AssetListDto
{
    public ulong Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string DeptName { get; set; } = string.Empty;
    public uint Quantity { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal RemainingValue { get; set; }
}

