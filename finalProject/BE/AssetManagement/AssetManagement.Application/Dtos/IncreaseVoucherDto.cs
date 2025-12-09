namespace AssetManagement.Application.Dtos;

public class IncreaseVoucherDto
{
    public ulong Id { get; set; }
    public string VoucherNo { get; set; } = string.Empty;
    public DateOnly VoucherDate { get; set; }
    public DateOnly IncreaseDate { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<IncreaseVoucherDetailDto> Details { get; set; } = new();
}

