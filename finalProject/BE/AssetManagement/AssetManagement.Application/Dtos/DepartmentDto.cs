namespace AssetManagement.Application.Dtos;

public class DepartmentDto
{
    public uint Id { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentSymbol { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
}

