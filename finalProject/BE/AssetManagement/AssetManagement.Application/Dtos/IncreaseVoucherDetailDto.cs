namespace AssetManagement.Application.Dtos;

public class IncreaseVoucherDetailDto
{
    public ulong Id { get; set; }
    public ulong VoucherId { get; set; }
    public ulong AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

