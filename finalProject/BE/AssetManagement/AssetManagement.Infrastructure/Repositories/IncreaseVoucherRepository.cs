using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Data;
using Dapper;
using System.Data;
using System.Linq;

namespace AssetManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho quản lý increase vouchers sử dụng Dapper
/// Kế thừa từ BaseRepository để tái sử dụng CRUD operations tự động
/// </summary>
public class IncreaseVoucherRepository : BaseRepository<AssetIncreaseVoucher, ulong>, IIncreaseVoucherRepository
{
    private const string VoucherColumns = @"
        v.id AS Id, v.voucher_no AS VoucherNo, v.voucher_date AS VoucherDate,
        v.increase_date AS IncreaseDate, v.note AS Note,
        v.created_at AS CreatedAt, v.updated_at AS UpdatedAt";

    /// <summary>
    /// Tên bảng trong database
    /// </summary>
    protected override string TableName => "asset_increase_vouchers";

    /// <summary>
    /// Các cột cần select (với alias v để dùng trong JOIN)
    /// </summary>
    protected override string SelectColumns => VoucherColumns;

    /// <summary>
    /// Override GetAllPagedAsync để thêm alias "v" vào FROM clause
    /// </summary>
    public override async Task<(IEnumerable<AssetIncreaseVoucher> Entities, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, string? orderBy = null)
    {
        var (validPageNumber, validPageSize) = ValidatePagination(pageNumber, pageSize);
        var offset = CalculateOffset(validPageNumber, validPageSize);
        var orderByClause = string.IsNullOrWhiteSpace(orderBy) ? $"ORDER BY {IdColumnName} DESC" : $"ORDER BY {orderBy}";

        var countSql = $"SELECT COUNT(*) FROM {TableName}";
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} v
            {orderByClause}
            LIMIT @Offset, @PageSize";

        using var connection = CreateConnection();
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var entities = await connection.QueryAsync<AssetIncreaseVoucher>(sql, new { Offset = offset, PageSize = validPageSize });

        return (entities, totalCount);
    }

    /// <summary>
    /// Constructor của IncreaseVoucherRepository
    /// </summary>
    /// <param name="connectionFactory">Factory để tạo database connection</param>
    public IncreaseVoucherRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    // ============================================
    // CRUD Operations - Dùng base methods tự động
    // ============================================

    /// <summary>
    /// Override GetByIdAsync để thêm alias "v" vào FROM clause (vì SelectColumns sử dụng alias v)
    /// </summary>
    public override async Task<AssetIncreaseVoucher?> GetByIdAsync(ulong id)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName} v
            WHERE v.{IdColumnName} = @Id";

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<AssetIncreaseVoucher>(sql, new { Id = id });
    }

    /// <summary>
    /// Lấy voucher theo ID kèm theo Details và Assets
    /// </summary>
    public async Task<AssetIncreaseVoucher?> GetById(ulong id)
    {
        // Dùng base method để lấy voucher
        var voucher = await GetByIdAsync(id);
        if (voucher == null) return null;
        
        // Load details với assets (logic đặc biệt)
        voucher.Details = (await GetDetailsByVoucherId(id)).ToList();
        return voucher;
    }

    /// <summary>
    /// Override AddAsync để chỉ insert các cột thực sự tồn tại trong database
    /// (bỏ qua Details, TotalOriginalPrice và DetailCount - computed/navigation properties)
    /// </summary>
    public override async Task<AssetIncreaseVoucher> AddAsync(AssetIncreaseVoucher entity)
    {
        // Set timestamps
        entity.CreatedAt = DateTime.Now;
        entity.UpdatedAt = DateTime.Now;

        // Chỉ insert các cột thực sự tồn tại trong database
        const string sql = @"
            INSERT INTO asset_increase_vouchers (voucher_no, voucher_date, increase_date, note, created_at, updated_at)
            VALUES (@VoucherNo, @VoucherDate, @IncreaseDate, @Note, @CreatedAt, @UpdatedAt);
            SELECT LAST_INSERT_ID();";

        using var connection = CreateConnection();
        var id = await connection.ExecuteScalarAsync<ulong>(sql, new
        {
            entity.VoucherNo,
            entity.VoucherDate,
            entity.IncreaseDate,
            entity.Note,
            entity.CreatedAt,
            entity.UpdatedAt
        });

        // Set ID vào entity
        entity.Id = id;

        return entity;
    }

    /// <summary>
    /// Thêm voucher mới vào database
    /// </summary>
    public async Task<AssetIncreaseVoucher> Add(AssetIncreaseVoucher voucher)
    {
        return await AddAsync(voucher);
    }

    /// <summary>
    /// Override UpdateAsync để chỉ update các cột thực sự tồn tại trong database
    /// (bỏ qua TotalOriginalPrice và DetailCount - computed properties)
    /// </summary>
    public override async Task<AssetIncreaseVoucher> UpdateAsync(AssetIncreaseVoucher entity)
    {
        // Set UpdatedAt
        entity.UpdatedAt = DateTime.Now;

        // Chỉ update các cột thực sự tồn tại trong database
        const string sql = @"
            UPDATE asset_increase_vouchers
            SET voucher_no = @VoucherNo,
                voucher_date = @VoucherDate,
                increase_date = @IncreaseDate,
                note = @Note,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            entity.Id,
            entity.VoucherNo,
            entity.VoucherDate,
            entity.IncreaseDate,
            entity.Note,
            entity.UpdatedAt
        });

        return entity;
    }

    /// <summary>
    /// Cập nhật voucher trong database
    /// </summary>
    public async Task<AssetIncreaseVoucher> Update(AssetIncreaseVoucher voucher)
    {
        return await UpdateAsync(voucher);
    }

    /// <summary>
    /// Xóa voucher theo ID
    /// Override để xóa details trước (logic đặc biệt)
    /// </summary>
    public async Task<bool> Delete(ulong id)
    {
        // Xóa details trước (cascade delete)
        await DeleteDetailsByVoucherId(id);
        // Sau đó xóa voucher (dùng base method)
        return await DeleteAsync(id);
    }

    // ============================================
    // Custom Methods - Logic đặc biệt cho Voucher
    // ============================================

    /// <summary>
    /// Lấy vouchers có phân trang từ database
    /// Tính tổng nguyên giá trực tiếp trong SQL để tối ưu hiệu suất
    /// </summary>
    public async Task<(IEnumerable<AssetIncreaseVoucher> Vouchers, int TotalCount)> GetAllPaged(int pageNumber, int pageSize)
    {
        var (validPageNumber, validPageSize) = ValidatePagination(pageNumber, pageSize);
        var offset = CalculateOffset(validPageNumber, validPageSize);

        var countSql = $"SELECT COUNT(*) FROM {TableName}";
        
        // Query với subquery để tính tổng nguyên giá và số lượng details trực tiếp trong SQL
        var sql = $@"
            SELECT 
                {VoucherColumns},
                COALESCE((
                    SELECT COUNT(*)
                    FROM asset_increase_voucher_details d
                    WHERE d.voucher_id = v.id
                ), 0) AS DetailCount,
                COALESCE((
                    SELECT SUM(a.purchase_price)
                    FROM asset_increase_voucher_details d
                    INNER JOIN assets a ON d.asset_id = a.id
                    WHERE d.voucher_id = v.id
                ), 0) AS TotalOriginalPrice
            FROM {TableName} v
            ORDER BY v.created_at DESC
            LIMIT @Offset, @PageSize";

        using var connection = CreateConnection();
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        
        // Query với dynamic để nhận TotalOriginalPrice và DetailCount
        var results = await connection.QueryAsync<dynamic>(sql, new { Offset = offset, PageSize = validPageSize });
        
        // Map vào entities và set TotalOriginalPrice, DetailCount
        var voucherList = results.Select(r => 
        {
            // Parse DateOnly từ DateTime hoặc DateOnly
            DateOnly ParseDateOnly(object? value)
            {
                if (value == null) return default;
                if (value is DateOnly dateOnly) return dateOnly;
                if (value is DateTime dateTime) return DateOnly.FromDateTime(dateTime);
                if (value is string str && DateTime.TryParse(str, out var dt)) return DateOnly.FromDateTime(dt);
                return default;
            }

            return new AssetIncreaseVoucher
            {
                Id = Convert.ToUInt64(r.Id),
                VoucherNo = (string)r.VoucherNo,
                VoucherDate = ParseDateOnly(r.VoucherDate),
                IncreaseDate = ParseDateOnly(r.IncreaseDate),
                Note = r.Note != null ? (string)r.Note : null,
                CreatedAt = r.CreatedAt is DateTime dt ? dt : DateTime.Parse(r.CreatedAt.ToString()!),
                UpdatedAt = r.UpdatedAt is DateTime dt2 ? dt2 : DateTime.Parse(r.UpdatedAt.ToString()!),
                TotalOriginalPrice = r.TotalOriginalPrice != null ? Convert.ToDecimal(r.TotalOriginalPrice) : 0,
                DetailCount = r.DetailCount != null ? Convert.ToInt32(r.DetailCount) : 0
            };
        }).ToList();

        return (voucherList, totalCount);
    }

    /// <summary>
    /// Lọc và lấy vouchers có phân trang với các điều kiện tìm kiếm
    /// </summary>
    public async Task<(IEnumerable<AssetIncreaseVoucher> Vouchers, int TotalCount)> Filter(
        int pageNumber, 
        int pageSize, 
        string? searchText = null, 
        DateOnly? fromDate = null, 
        DateOnly? toDate = null,
        decimal? totalOriginalPrice = null)
    {
        // Xây dựng WHERE clause động
        var whereConditions = new List<string>();
        var parameters = new Dictionary<string, object?>();

        // Tìm kiếm theo số chứng từ hoặc ghi chú
        var searchPattern = CreateLikePattern(searchText);
        if (searchPattern != null)
        {
            whereConditions.Add("(v.voucher_no LIKE @SearchText OR v.note LIKE @SearchText)");
            parameters["SearchText"] = searchPattern;
        }

        // Lọc theo ngày bắt đầu
        if (fromDate.HasValue)
        {
            whereConditions.Add("v.voucher_date >= @FromDate");
            parameters["FromDate"] = fromDate.Value;
        }

        // Lọc theo ngày kết thúc
        if (toDate.HasValue)
        {
            whereConditions.Add("v.voucher_date <= @ToDate");
            parameters["ToDate"] = toDate.Value;
        }

        // Lọc theo tổng nguyên giá (exact match với dấu =)
        if (totalOriginalPrice.HasValue)
        {
            whereConditions.Add(@"(SELECT SUM(a.purchase_price)
                    FROM asset_increase_voucher_details d
                    INNER JOIN assets a ON d.asset_id = a.id
                    WHERE d.voucher_id = v.id) = @TotalOriginalPrice");
            parameters["TotalOriginalPrice"] = totalOriginalPrice.Value;
        }

        var whereClause = BuildWhereClause(whereConditions);
        var offset = CalculateOffset(pageNumber, pageSize);

        // Count SQL
        var countSql = $@"
            SELECT COUNT(*) 
            FROM asset_increase_vouchers v
            {whereClause}";

        // Main SQL với subquery để tính tổng nguyên giá và số lượng details trực tiếp trong SQL
        var sql = $@"
            SELECT 
                {VoucherColumns},
                COALESCE((
                    SELECT COUNT(*)
                    FROM asset_increase_voucher_details d
                    WHERE d.voucher_id = v.id
                ), 0) AS DetailCount,
                COALESCE((
                    SELECT SUM(a.purchase_price)
                    FROM asset_increase_voucher_details d
                    INNER JOIN assets a ON d.asset_id = a.id
                    WHERE d.voucher_id = v.id
                ), 0) AS TotalOriginalPrice
            FROM asset_increase_vouchers v
            {whereClause}
            ORDER BY v.created_at DESC
            LIMIT @Offset, @PageSize";

        using var connection = CreateConnection();
        
        // Tạo anonymous object từ dictionary để Dapper có thể sử dụng
        var paramObject = new
        {
            SearchText = parameters.ContainsKey("SearchText") ? parameters["SearchText"] : null,
            FromDate = parameters.ContainsKey("FromDate") ? parameters["FromDate"] : null,
            ToDate = parameters.ContainsKey("ToDate") ? parameters["ToDate"] : null,
            TotalOriginalPrice = parameters.ContainsKey("TotalOriginalPrice") ? parameters["TotalOriginalPrice"] : null,
            Offset = offset,
            PageSize = pageSize
        };
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, paramObject);
        
        // Query với dynamic để nhận TotalOriginalPrice và DetailCount
        var results = await connection.QueryAsync<dynamic>(sql, paramObject);
        
        // Map vào entities và set TotalOriginalPrice, DetailCount
        var voucherList = results.Select(r => 
        {
            // Parse DateOnly từ DateTime hoặc DateOnly
            DateOnly ParseDateOnly(object? value)
            {
                if (value == null) return default;
                if (value is DateOnly dateOnly) return dateOnly;
                if (value is DateTime dateTime) return DateOnly.FromDateTime(dateTime);
                if (value is string str && DateTime.TryParse(str, out var dt)) return DateOnly.FromDateTime(dt);
                return default;
            }

            return new AssetIncreaseVoucher
            {
                Id = Convert.ToUInt64(r.Id),
                VoucherNo = (string)r.VoucherNo,
                VoucherDate = ParseDateOnly(r.VoucherDate),
                IncreaseDate = ParseDateOnly(r.IncreaseDate),
                Note = r.Note != null ? (string)r.Note : null,
                CreatedAt = r.CreatedAt is DateTime dt ? dt : DateTime.Parse(r.CreatedAt.ToString()!),
                UpdatedAt = r.UpdatedAt is DateTime dt2 ? dt2 : DateTime.Parse(r.UpdatedAt.ToString()!),
                DetailCount = r.DetailCount != null ? Convert.ToInt32(r.DetailCount) : 0,
                TotalOriginalPrice = r.TotalOriginalPrice != null ? Convert.ToDecimal(r.TotalOriginalPrice) : 0
            };
        }).ToList();

        return (voucherList, totalCount);
    }

    /// <summary>
    /// Lấy voucher theo số chứng từ
    /// </summary>
    public async Task<AssetIncreaseVoucher?> GetByVoucherNo(string voucherNo)
    {
        var sql = $@"
            SELECT {VoucherColumns}
            FROM asset_increase_vouchers v
            WHERE v.voucher_no = @VoucherNo";

        using var connection = CreateConnection();
        var voucher = await connection.QueryFirstOrDefaultAsync<AssetIncreaseVoucher>(
            sql, new { VoucherNo = voucherNo });
        
        if (voucher == null) return null;

        // Load details với assets
        voucher.Details = (await GetDetailsByVoucherId(voucher.Id)).ToList();
        return voucher;
    }

    /// <summary>
    /// Kiểm tra xem số chứng từ đã tồn tại trong database chưa
    /// </summary>
    public async Task<bool> ExistsVoucherNo(string voucherNo)
    {
        const string sql = "SELECT COUNT(*) FROM asset_increase_vouchers WHERE voucher_no = @VoucherNo";

        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { VoucherNo = voucherNo });
        
        return count > 0;
    }

    /// <summary>
    /// Lấy số chứng từ lớn nhất theo prefix (ví dụ: "GT" -> "GT000019")
    /// </summary>
    public async Task<string?> GetMaxVoucherNoByPrefix(string prefix)
    {
        const string sql = @"
            SELECT voucher_no 
            FROM asset_increase_vouchers 
            WHERE voucher_no LIKE @Pattern
            ORDER BY 
                CAST(SUBSTRING(voucher_no, @PrefixLength + 1) AS UNSIGNED) DESC,
                voucher_no DESC
            LIMIT 1";

        using var connection = CreateConnection();
        var pattern = $"{prefix}%";
        var prefixLength = prefix.Length;
        
        var maxCode = await connection.QueryFirstOrDefaultAsync<string>(sql, new 
        { 
            Pattern = pattern,
            PrefixLength = prefixLength
        });
        
        return maxCode;
    }
//------------------------------------------------------------------------------------------------

    /// <summary>
    /// Lấy tất cả details của một voucher kèm theo thông tin Asset
    /// </summary>
    public async Task<IEnumerable<AssetIncreaseVoucherDetail>> GetDetailsByVoucherId(ulong voucherId)
    {
        const string sql = @"
            SELECT 
                d.id AS Id, d.voucher_id AS VoucherId, d.asset_id AS AssetId, d.created_at AS CreatedAt,
                a.id AS AssetId, a.asset_code AS AssetCode, a.asset_name AS AssetName,
                a.asset_symbol AS AssetSymbol, a.department_id AS DepartmentId, a.type_id AS TypeId,
                a.purchase_date AS PurchaseDate, a.purchase_year AS PurchaseYear,
                a.start_tracking_year AS StartTrackingYear, a.years_of_use AS YearsOfUse,
                a.depreciation_rate AS DepreciationRate, a.quantity AS Quantity,
                a.purchase_price AS PurchasePrice, a.annual_depreciation_value AS AnnualDepreciationValue,
                a.created_at AS AssetCreatedAt, a.updated_at AS UpdatedAt
            FROM asset_increase_voucher_details d
            INNER JOIN assets a ON d.asset_id = a.id
            WHERE d.voucher_id = @VoucherId
            ORDER BY d.created_at";

        using var connection = CreateConnection();
        // Query với dynamic để tránh conflict tên cột
        var rows = await connection.QueryAsync(sql, new { VoucherId = voucherId });
        
        var result = new List<AssetIncreaseVoucherDetail>();
        foreach (var row in rows)
        {
            // Parse PurchaseDate - có thể là DateOnly hoặc DateTime
            DateOnly purchaseDate;
            if (row.PurchaseDate is DateOnly dateOnly)
            {
                purchaseDate = dateOnly;
            }
            else if (row.PurchaseDate is DateTime dateTime)
            {
                purchaseDate = DateOnly.FromDateTime(dateTime);
            }
            else
            {
                purchaseDate = DateOnly.FromDateTime(DateTime.Parse(row.PurchaseDate.ToString()!));
            }

            var detail = new AssetIncreaseVoucherDetail
            {
                Id = (ulong)row.Id,
                VoucherId = (ulong)row.VoucherId,
                AssetId = (ulong)row.AssetId,
                CreatedAt = (DateTime)row.CreatedAt,
                Asset = new Asset
                {
                    Id = (ulong)row.AssetId,
                    AssetCode = (string)row.AssetCode,
                    AssetName = (string)row.AssetName,
                    AssetSymbol = (string)row.AssetSymbol,
                    DepartmentId = (uint)row.DepartmentId,
                    TypeId = (uint)row.TypeId,
                    PurchaseDate = purchaseDate,
                    PurchaseYear = row.PurchaseYear != null ? (short?)row.PurchaseYear : null,
                    StartTrackingYear = row.StartTrackingYear != null ? (short?)row.StartTrackingYear : null,
                    YearsOfUse = (uint)row.YearsOfUse,
                    DepreciationRate = (decimal)row.DepreciationRate,
                    Quantity = (uint)row.Quantity,
                    PurchasePrice = (decimal)row.PurchasePrice,
                    AnnualDepreciationValue = row.AnnualDepreciationValue != null ? (decimal?)row.AnnualDepreciationValue : null,
                    CreatedAt = (DateTime)row.AssetCreatedAt,
                    UpdatedAt = (DateTime)row.UpdatedAt
                }
            };
            result.Add(detail);
        }

        return result;
    }

    /// <summary>
    /// Xóa tất cả details của một voucher
    /// </summary>
    public async Task<int> DeleteDetailsByVoucherId(ulong voucherId)
    {
        const string sql = "DELETE FROM asset_increase_voucher_details WHERE voucher_id = @VoucherId";

        using var connection = CreateConnection();
        return await connection.ExecuteAsync(sql, new { VoucherId = voucherId });
    }

    /// <summary>
    /// Xóa một detail theo ID
    /// </summary>
    public async Task<bool> DeleteDetail(ulong detailId)
    {
        const string sql = "DELETE FROM asset_increase_voucher_details WHERE id = @DetailId";

        using var connection = CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { DetailId = detailId });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Thêm detail mới vào database
    /// </summary>
    public async Task<AssetIncreaseVoucherDetail> AddDetail(AssetIncreaseVoucherDetail detail)
    {
        const string sql = @"
            INSERT INTO asset_increase_voucher_details (
                voucher_id, asset_id, created_at
            ) VALUES (
                @VoucherId, @AssetId, @CreatedAt
            );
            SELECT LAST_INSERT_ID();";

        using var connection = CreateConnection();
        
        var id = await connection.ExecuteScalarAsync<ulong>(sql, new
        {
            detail.VoucherId,
            detail.AssetId,
            detail.CreatedAt
        });

        detail.Id = id;
        return detail;
    }

    /// <summary>
    /// Xóa nhiều vouchers theo danh sách IDs
    /// </summary>
    public async Task<int> DeleteVouchersByIds(IEnumerable<ulong> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return 0;

        using var connection = CreateConnection();
        
        // Xóa details trước (cascade delete)
        const string deleteDetailsSql = "DELETE FROM asset_increase_voucher_details WHERE voucher_id IN @Ids";
        await connection.ExecuteAsync(deleteDetailsSql, new { Ids = idList });
        
        // Sau đó xóa vouchers
        const string deleteVouchersSql = "DELETE FROM asset_increase_vouchers WHERE id IN @Ids";
        return await connection.ExecuteAsync(deleteVouchersSql, new { Ids = idList });
    }
}
