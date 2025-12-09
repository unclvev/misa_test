using System;
using System.Collections.Generic;

namespace AssetManagement.Domain.Entities;

public partial class Department
{
    public uint Id { get; set; }

    public string DepartmentCode { get; set; } = null!;

    public string DepartmentSymbol { get; set; } = null!;

    public string DepartmentName { get; set; } = null!;

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
