using System;
using System.Collections.Generic;

namespace AssetManagement.Domain.Entities;

public partial class AssetIncreaseVoucherDetail
{
    public ulong Id { get; set; }

    public ulong VoucherId { get; set; }

    public ulong AssetId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AssetIncreaseVoucher Voucher { get; set; } = null!;

    public virtual Asset Asset { get; set; } = null!;
}

