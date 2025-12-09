namespace AssetManagement.Application.Dtos;

public class IncreaseVoucherListDto
{
    public ulong Id { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateOnly VoucherDate { get; set; }
    public DateOnly IncreaseDate { get; set; }
    public string? Note { get; set; }
    public int DetailCount { get; set; }
    public decimal TotalOriginalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

