using System;
using System.Collections.Generic;

namespace AssetManagement.Domain.Entities;

public partial class AssetType
{
    public uint Id { get; set; }

    public byte TypeCode { get; set; }

    public string TypeSymbol { get; set; } = null!;

    public string TypeName { get; set; } = null!;

    public uint YearsOfUse { get; set; }

    public decimal DepreciationRate { get; set; }

    public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
