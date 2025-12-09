namespace AssetManagement.Application.Dtos;

public class AssetTypeDto
{
    public uint Id { get; set; }
    public byte TypeCode { get; set; }
    public string TypeSymbol { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public uint YearsOfUse { get; set; }
    public decimal DepreciationRate { get; set; }
}

