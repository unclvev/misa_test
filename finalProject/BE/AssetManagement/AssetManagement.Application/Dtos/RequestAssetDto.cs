using System.ComponentModel.DataAnnotations;

namespace AssetManagement.Application.Dtos;

public class RequestAssetDto
{
    [StringLength(64, ErrorMessage = "Mã tài sản không được vượt quá 64 ký tự")]
    public string AssetCode { get; set; } = string.Empty; // Nếu để trống sẽ tự động sinh mã

    [Required(ErrorMessage = "Tên tài sản không được để trống")]
    [StringLength(255, ErrorMessage = "Tên tài sản không được vượt quá 255 ký tự")]
    public string AssetName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã bộ phận sử dụng không được để trống")]
    public string DeptCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mã loại tài sản không được để trống")]
    public byte TypeCode { get; set; }

    [Required(ErrorMessage = "Ngày mua không được để trống")]
    public DateOnly PurchaseDate { get; set; }

    [Required(ErrorMessage = "Số lượng không được để trống")]
    [Range(1, uint.MaxValue, ErrorMessage = "Số lượng phải là số nguyên dương")]
    public uint Quantity { get; set; }

    [Required(ErrorMessage = "Nguyên giá không được để trống")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Nguyên giá phải lớn hơn 0")]
    public decimal PurchasePrice { get; set; }
}

