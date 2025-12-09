using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.Dtos;

public class RequestIncreaseVoucherDetailDto
{
    [Required(ErrorMessage = "AssetId không được để trống")]
    public ulong AssetId { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

public class RequestIncreaseVoucherDto
{
    [StringLength(64, ErrorMessage = "Số chứng từ không được vượt quá 64 ký tự")]
    public string VoucherNo { get; set; } = string.Empty; // Nếu để trống sẽ tự động sinh mã

    [Required(ErrorMessage = "Ngày chứng từ không được để trống")]
    public DateOnly VoucherDate { get; set; }

    [Required(ErrorMessage = "Ngày ghi tăng không được để trống")]
    public DateOnly IncreaseDate { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
    public string? Note { get; set; }

    // Details có thể rỗng khi tạo voucher mới (sẽ thêm assets sau)
    public List<RequestIncreaseVoucherDetailDto>? Details { get; set; }
}

