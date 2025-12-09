using AssetManagement.Application.Interfaces;
using AssetManagement.Domain.Entities;
using AssetManagement.Infrastructure.Data;
using Dapper;
using System.Data;

namespace AssetManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation cho quản lý assets sử dụng Dapper
/// </summary>
public class AssetRepository : IAssetRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    /// <summary>
    /// Static constructor để đăng ký DateOnly type handler cho Dapper
    /// </summary>
    static AssetRepository()
    {
        // Đăng ký DateOnly type handler cho Dapper
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }
    
    private const string AssetColumns = @"
        a.id AS Id, a.asset_code AS AssetCode, a.asset_symbol AS AssetSymbol, 
        a.asset_name AS AssetName, a.department_id AS DepartmentId, a.type_id AS TypeId,
        a.purchase_date AS PurchaseDate, a.purchase_year AS PurchaseYear, 
        a.start_tracking_year AS StartTrackingYear, a.years_of_use AS YearsOfUse,
        a.depreciation_rate AS DepreciationRate, a.quantity AS Quantity,
        a.purchase_price AS PurchasePrice, a.annual_depreciation_value AS AnnualDepreciationValue,
        a.created_at AS CreatedAt, a.updated_at AS UpdatedAt";
    
    private const string DepartmentColumns = @"
        d.id AS DepartmentId, d.department_code AS DepartmentCode, 
        d.department_symbol AS DepartmentSymbol, d.department_name AS DepartmentName";
    
    private const string AssetTypeColumns = @"
        t.id AS TypeId, t.type_code AS TypeCode, t.type_symbol AS TypeSymbol, 
        t.type_name AS TypeName, t.years_of_use AS YearsOfUse, 
        t.depreciation_rate AS DepreciationRate";

    /// <summary>
    /// Constructor của AssetRepository
    /// </summary>
    /// <param name="connectionFactory">Factory để tạo database connection</param>
    public AssetRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Map asset với các quan hệ (Department và AssetType) từ kết quả query
    /// </summary>
    /// <param name="asset">Asset entity cần map</param>
    /// <param name="dept">Dữ liệu department từ query</param>
    /// <param name="assetType">Dữ liệu asset type từ query</param>
    /// <returns>Asset đã được map với Department và AssetType</returns>
    private static Asset MapAssetWithRelations(Asset asset, dynamic dept, dynamic assetType)
    {
        var department = new Department
        {
            Id = (uint)dept.DepartmentId,
            DepartmentCode = dept.DepartmentCode,
            DepartmentSymbol = dept.DepartmentSymbol,
            DepartmentName = dept.DepartmentName
        };
        var type = new AssetType
        {
            Id = (uint)assetType.TypeId,
            TypeCode = assetType.TypeCode,
            TypeSymbol = assetType.TypeSymbol,
            TypeName = assetType.TypeName,
            YearsOfUse = assetType.YearsOfUse,
            DepreciationRate = assetType.DepreciationRate
        };
        asset.Department = department;
        asset.Type = type;
        return asset;
    }

    /// <summary>
    /// Lấy tất cả assets từ database kèm theo Department và AssetType
    /// </summary>
    /// <returns>Danh sách tất cả assets, sắp xếp theo created_at DESC</returns>
    public async Task<IEnumerable<Asset>> GetAll()
    {
        var sql = $@"
            SELECT {AssetColumns},
                   {DepartmentColumns},
                   {AssetTypeColumns}
            FROM assets a
            INNER JOIN departments d ON a.department_id = d.id
            INNER JOIN asset_types t ON a.type_id = t.id
            ORDER BY a.created_at DESC";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<Asset, dynamic, dynamic, Asset>(
            sql,
            MapAssetWithRelations,
            splitOn: "DepartmentId,TypeId"
        );

        return result;
    }

    /// <summary>
    /// Lấy assets có phân trang từ database
    /// </summary>
    /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
    /// <param name="pageSize">Số lượng records mỗi trang</param>
    /// <returns>Tuple chứa danh sách assets và tổng số records</returns>
    public async Task<(IEnumerable<Asset> Assets, int TotalCount)> GetAllPaged(int pageNumber, int pageSize)
    {
        const string countSql = "SELECT COUNT(*) FROM assets";
        var sql = $@"
            SELECT {AssetColumns},
                   {DepartmentColumns},
                   {AssetTypeColumns}
            FROM assets a
            INNER JOIN departments d ON a.department_id = d.id
            INNER JOIN asset_types t ON a.type_id = t.id
            ORDER BY a.created_at DESC
            LIMIT @Offset, @PageSize";

        using var connection = _connectionFactory.CreateConnection();
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        
        var offset = (pageNumber - 1) * pageSize;
        var assets = await connection.QueryAsync<Asset, dynamic, dynamic, Asset>(
            sql,
            MapAssetWithRelations,
            new { Offset = offset, PageSize = pageSize },
            splitOn: "DepartmentId,TypeId"
        );

        return (assets, totalCount);
    }

    /// <summary>
    /// Lọc và lấy assets có phân trang với các điều kiện tìm kiếm
    /// </summary>
    /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
    /// <param name="pageSize">Số lượng records mỗi trang</param>
    /// <param name="searchText">Từ khóa tìm kiếm (tìm trong asset_code và asset_name)</param>
    /// <param name="deptCode">Mã bộ phận sử dụng để lọc</param>
    /// <param name="typeCode">Mã loại tài sản để lọc</param>
    /// <returns>Tuple chứa danh sách assets đã lọc và tổng số records</returns>
    public async Task<(IEnumerable<Asset> Assets, int TotalCount)> Filter(int pageNumber, int pageSize, string? searchText = null, string? deptCode = null, byte? typeCode = null)
    {
        // Xây dựng WHERE clause động
        var whereConditions = new List<string>();
        var parameters = new Dictionary<string, object?>();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            whereConditions.Add("(a.asset_code LIKE @SearchText OR a.asset_name LIKE @SearchText)");
            parameters["SearchText"] = $"%{searchText}%";
        }

        if (!string.IsNullOrWhiteSpace(deptCode))
        {
            whereConditions.Add("d.department_code = @DeptCode");
            parameters["DeptCode"] = deptCode;
        }

        if (typeCode.HasValue && typeCode.Value > 0)
        {
            whereConditions.Add("t.type_code = @TypeCode");
            parameters["TypeCode"] = typeCode.Value;
        }

        var whereClause = whereConditions.Count > 0 
            ? "WHERE " + string.Join(" AND ", whereConditions)
            : "";

        var offset = (pageNumber - 1) * pageSize;
        parameters["Offset"] = offset;
        parameters["PageSize"] = pageSize;

        // Count SQL
        var countSql = $@"
            SELECT COUNT(*) 
            FROM assets a
            INNER JOIN departments d ON a.department_id = d.id
            INNER JOIN asset_types t ON a.type_id = t.id
            {whereClause}";

        // Main SQL
        var sql = $@"
            SELECT {AssetColumns},
                   {DepartmentColumns},
                   {AssetTypeColumns}
            FROM assets a
            INNER JOIN departments d ON a.department_id = d.id
            INNER JOIN asset_types t ON a.type_id = t.id
            {whereClause}
            ORDER BY a.created_at DESC
            LIMIT @Offset, @PageSize";

        using var connection = _connectionFactory.CreateConnection();
        
        // Tạo anonymous object từ dictionary để Dapper có thể sử dụng
        var paramObject = new
        {
            SearchText = parameters.ContainsKey("SearchText") ? parameters["SearchText"] : null,
            DeptCode = parameters.ContainsKey("DeptCode") ? parameters["DeptCode"] : null,
            TypeCode = parameters.ContainsKey("TypeCode") ? parameters["TypeCode"] : null,
            Offset = offset,
            PageSize = pageSize
        };
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, paramObject);
        
        var assets = await connection.QueryAsync<Asset, dynamic, dynamic, Asset>(
            sql,
            MapAssetWithRelations,
            paramObject,
            splitOn: "DepartmentId,TypeId"
        );

        return (assets, totalCount);
    }

    /// <summary>
    /// Lấy asset theo ID kèm theo Department và AssetType
    /// </summary>
    /// <param name="id">ID của asset cần lấy</param>
    /// <returns>Asset nếu tìm thấy, null nếu không tìm thấy</returns>
    public async Task<Asset?> GetById(ulong id)
    {
        var sql = $@"
            SELECT {AssetColumns},
                   {DepartmentColumns},
                   {AssetTypeColumns}
            FROM assets a
            INNER JOIN departments d ON a.department_id = d.id
            INNER JOIN asset_types t ON a.type_id = t.id
            WHERE a.id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<Asset, dynamic, dynamic, Asset>(
            sql,
            MapAssetWithRelations,
            new { Id = id },
            splitOn: "DepartmentId,TypeId"
        );

        return result.FirstOrDefault();
    }

    /// <summary>
    /// Lấy asset theo mã tài sản (asset_code) kèm theo Department và AssetType
    /// </summary>
    /// <param name="assetCode">Mã tài sản cần tìm</param>
    /// <returns>Asset nếu tìm thấy, null nếu không tìm thấy</returns>
    public async Task<Asset?> GetByAssetCode(string assetCode)
    {
        var sql = $@"
            SELECT {AssetColumns},
                   {DepartmentColumns},
                   {AssetTypeColumns}
            FROM assets a
            INNER JOIN departments d ON a.department_id = d.id
            INNER JOIN asset_types t ON a.type_id = t.id
            WHERE a.asset_code = @AssetCode";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryAsync<Asset, dynamic, dynamic, Asset>(
            sql,
            MapAssetWithRelations,
            new { AssetCode = assetCode },
            splitOn: "DepartmentId,TypeId"
        );

        return result.FirstOrDefault();
    }

    /// <summary>
    /// Thêm asset mới vào database
    /// </summary>
    /// <param name="asset">Asset entity cần thêm</param>
    /// <returns>Asset đã được thêm với ID mới và các quan hệ đã load</returns>
    public async Task<Asset> Add(Asset asset)
    {
        const string sql = @"
            INSERT INTO assets (
                asset_code, asset_symbol, asset_name, department_id, type_id, 
                purchase_date, years_of_use, depreciation_rate, quantity, purchase_price, 
                created_at, updated_at
            ) VALUES (
                @AssetCode, @AssetSymbol, @AssetName, @DepartmentId, @TypeId,
                @PurchaseDate, @YearsOfUse, @DepreciationRate, @Quantity, @PurchasePrice,
                @CreatedAt, @UpdatedAt
            );
            SELECT LAST_INSERT_ID();";

        using var connection = _connectionFactory.CreateConnection();
        asset.CreatedAt = DateTime.Now;
        asset.UpdatedAt = DateTime.Now;
        
        var id = await connection.ExecuteScalarAsync<ulong>(sql, new
        {
            asset.AssetCode,
            asset.AssetSymbol,
            asset.AssetName,
            asset.DepartmentId,
            asset.TypeId,
            asset.PurchaseDate,
            asset.YearsOfUse,
            asset.DepreciationRate,
            asset.Quantity,
            asset.PurchasePrice,
            asset.CreatedAt,
            asset.UpdatedAt
        });

        asset.Id = id;
        return await GetById(id) ?? asset;
    }

    /// <summary>
    /// Cập nhật asset trong database
    /// </summary>
    /// <param name="asset">Asset entity cần cập nhật (phải có ID)</param>
    /// <returns>Asset đã được cập nhật với các quan hệ đã load</returns>
    public async Task<Asset> Update(Asset asset)
    {
        const string sql = @"
            UPDATE assets SET
                asset_code = @AssetCode,
                asset_symbol = @AssetSymbol,
                asset_name = @AssetName,
                department_id = @DepartmentId,
                type_id = @TypeId,
                purchase_date = @PurchaseDate,
                years_of_use = @YearsOfUse,
                depreciation_rate = @DepreciationRate,
                quantity = @Quantity,
                purchase_price = @PurchasePrice,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        asset.UpdatedAt = DateTime.Now;
        
        await connection.ExecuteAsync(sql, new
        {
            asset.Id,
            asset.AssetCode,
            asset.AssetSymbol,
            asset.AssetName,
            asset.DepartmentId,
            asset.TypeId,
            asset.PurchaseDate,
            asset.YearsOfUse,
            asset.DepreciationRate,
            asset.Quantity,
            asset.PurchasePrice,
            asset.UpdatedAt
        });

        return await GetById(asset.Id) ?? asset;
    }

    /// <summary>
    /// Xóa asset theo ID
    /// </summary>
    /// <param name="id">ID của asset cần xóa</param>
    /// <returns>true nếu xóa thành công, false nếu không tìm thấy asset</returns>
    public async Task<bool> Delete(ulong id)
    {
        const string sql = "DELETE FROM assets WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        
        return rowsAffected > 0;
    }

    /// <summary>
    /// Kiểm tra xem mã tài sản đã tồn tại trong database chưa
    /// </summary>
    /// <param name="assetCode">Mã tài sản cần kiểm tra</param>
    /// <returns>true nếu mã đã tồn tại, false nếu chưa tồn tại</returns>
    public async Task<bool> ExistsAssetCode(string assetCode)
    {
        const string sql = "SELECT COUNT(*) FROM assets WHERE asset_code = @AssetCode";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { AssetCode = assetCode });
        
        return count > 0;
    }

    /// <summary>
    /// Lấy mã tài sản lớn nhất theo prefix (ví dụ: "TS" -> "TS00009")
    /// </summary>
    /// <param name="prefix">Prefix của mã tài sản (ví dụ: "TS")</param>
    /// <returns>Mã tài sản lớn nhất nếu tìm thấy, null nếu không có</returns>
    public async Task<string?> GetMaxAssetCodeByPrefix(string prefix)
    {
        const string sql = @"
            SELECT asset_code 
            FROM assets 
            WHERE asset_code LIKE @Pattern
            ORDER BY 
                CAST(SUBSTRING(asset_code, @PrefixLength + 1) AS UNSIGNED) DESC,
                asset_code DESC
            LIMIT 1";

        using var connection = _connectionFactory.CreateConnection();
        var pattern = $"{prefix}%";
        var prefixLength = prefix.Length;
        
        var maxCode = await connection.QueryFirstOrDefaultAsync<string>(sql, new 
        { 
            Pattern = pattern,
            PrefixLength = prefixLength
        });
        
        return maxCode;
    }

    /// <summary>
    /// Lấy department theo mã bộ phận
    /// </summary>
    /// <param name="deptCode">Mã bộ phận cần tìm</param>
    /// <returns>Department nếu tìm thấy, null nếu không tìm thấy</returns>
    public async Task<Department?> GetDepartmentByCode(string deptCode)
    {
        const string sql = @"
            SELECT id AS Id, department_code AS DepartmentCode, 
                   department_symbol AS DepartmentSymbol, department_name AS DepartmentName
            FROM departments
            WHERE department_code = @DeptCode";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Department>(sql, new { DeptCode = deptCode });
    }

    /// <summary>
    /// Lấy asset type theo mã loại tài sản
    /// </summary>
    /// <param name="typeCode">Mã loại tài sản cần tìm</param>
    /// <returns>AssetType nếu tìm thấy, null nếu không tìm thấy</returns>
    public async Task<AssetType?> GetAssetTypeByCode(byte typeCode)
    {
        const string sql = @"
            SELECT id AS Id, type_code AS TypeCode, type_symbol AS TypeSymbol, 
                   type_name AS TypeName, years_of_use AS YearsOfUse, 
                   depreciation_rate AS DepreciationRate
            FROM asset_types
            WHERE type_code = @TypeCode";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<AssetType>(sql, new { TypeCode = typeCode });
    }

    /// <summary>
    /// Lấy tất cả departments từ database
    /// </summary>
    /// <returns>Danh sách tất cả departments, sắp xếp theo department_code</returns>
    public async Task<IEnumerable<Department>> GetAllDepartments()
    {
        const string sql = @"
            SELECT id AS Id, department_code AS DepartmentCode, 
                   department_symbol AS DepartmentSymbol, department_name AS DepartmentName
            FROM departments
            ORDER BY department_code";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Department>(sql);
    }

    /// <summary>
    /// Lấy tất cả asset types từ database
    /// </summary>
    /// <returns>Danh sách tất cả asset types, sắp xếp theo type_code</returns>
    public async Task<IEnumerable<AssetType>> GetAllAssetTypes()
    {
        const string sql = @"
            SELECT id AS Id, type_code AS TypeCode, type_symbol AS TypeSymbol, 
                   type_name AS TypeName, years_of_use AS YearsOfUse, 
                   depreciation_rate AS DepreciationRate
            FROM asset_types
            ORDER BY type_code";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<AssetType>(sql);
    }

    /// <summary>
    /// Xóa nhiều assets theo danh sách IDs
    /// Chỉ xóa những assets không có trong chứng từ ghi tăng
    /// </summary>
    /// <param name="ids">Danh sách IDs của các assets cần xóa</param>
    /// <returns>Số lượng assets đã được xóa</returns>
    public async Task<int> DeleteAssetsByIds(IEnumerable<ulong> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) return 0;

        // Lọc ra những asset không có trong chứng từ ghi tăng
        var assetsCanDelete = new List<ulong>();
        
        foreach (var id in idList)
        {
            if (!await IsAssetInIncreaseVoucher(id))
            {
                assetsCanDelete.Add(id);
            }
        }

        if (assetsCanDelete.Count == 0) return 0;

        const string sql = "DELETE FROM assets WHERE id IN @Ids";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, new { Ids = assetsCanDelete });
    }

    /// <summary>
    /// Kiểm tra xem tài sản có trong chứng từ ghi tăng nào không
    /// </summary>
    /// <param name="assetId">ID của tài sản cần kiểm tra</param>
    /// <returns>true nếu tài sản có trong chứng từ ghi tăng, false nếu không</returns>
    public async Task<bool> IsAssetInIncreaseVoucher(ulong assetId)
    {
        const string sql = @"
            SELECT COUNT(*) 
            FROM asset_increase_voucher_details 
            WHERE asset_id = @AssetId";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { AssetId = assetId });
        
        return count > 0;
    }

    /// <summary>
    /// Lọc tài sản chưa có trong chứng từ phái sinh (chưa có trong asset_increase_voucher_details)
    /// </summary>
    /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
    /// <param name="pageSize">Số lượng bản ghi mỗi trang</param>
    /// <param name="searchText">Từ khóa tìm kiếm (tìm trong asset_code và asset_name)</param>
    /// <param name="deptCode">Mã bộ phận</param>
    /// <param name="typeCode">Mã loại tài sản</param>
    /// <returns>Tuple chứa danh sách assets và tổng số bản ghi</returns>
    public async Task<(IEnumerable<Asset> Assets, int TotalCount)> FilterAvailableForVoucher(int pageNumber, int pageSize, string? searchText = null, string? deptCode = null, byte? typeCode = null)
    {
        // Xây dựng WHERE clause động
        var whereConditions = new List<string>();
        var parameters = new Dictionary<string, object?>();

        // Điều kiện loại trừ tài sản đã có trong chứng từ phái sinh
        whereConditions.Add("a.id NOT IN (SELECT DISTINCT asset_id FROM asset_increase_voucher_details WHERE asset_id IS NOT NULL)");

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            whereConditions.Add("(a.asset_code LIKE @SearchText OR a.asset_name LIKE @SearchText)");
            parameters["SearchText"] = $"%{searchText}%";
        }

        if (!string.IsNullOrWhiteSpace(deptCode))
        {
            whereConditions.Add("d.department_code = @DeptCode");
            parameters["DeptCode"] = deptCode;
        }

        if (typeCode.HasValue && typeCode.Value > 0)
        {
            whereConditions.Add("t.type_code = @TypeCode");
            parameters["TypeCode"] = typeCode.Value;
        }

        var whereClause = whereConditions.Count > 0 
            ? "WHERE " + string.Join(" AND ", whereConditions)
            : "";

        var offset = (pageNumber - 1) * pageSize;
        parameters["Offset"] = offset;
        parameters["PageSize"] = pageSize;

        // Count SQL
        var countSql = $@"
            SELECT COUNT(*) 
            FROM assets a
            INNER JOIN departments d ON a.department_id = d.id
            INNER JOIN asset_types t ON a.type_id = t.id
            {whereClause}";

        // Main SQL
        var sql = $@"
            SELECT {AssetColumns},
                   {DepartmentColumns},
                   {AssetTypeColumns}
            FROM assets a
            INNER JOIN departments d ON a.department_id = d.id
            INNER JOIN asset_types t ON a.type_id = t.id
            {whereClause}
            ORDER BY a.created_at DESC
            LIMIT @Offset, @PageSize";

        using var connection = _connectionFactory.CreateConnection();
        
        // Tạo anonymous object từ dictionary để Dapper có thể sử dụng
        var paramObject = new
        {
            SearchText = parameters.ContainsKey("SearchText") ? parameters["SearchText"] : null,
            DeptCode = parameters.ContainsKey("DeptCode") ? parameters["DeptCode"] : null,
            TypeCode = parameters.ContainsKey("TypeCode") ? parameters["TypeCode"] : null,
            Offset = offset,
            PageSize = pageSize
        };
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, paramObject);
        
        var assets = await connection.QueryAsync<Asset, dynamic, dynamic, Asset>(
            sql,
            MapAssetWithRelations,
            paramObject,
            splitOn: "DepartmentId,TypeId"
        );

        return (assets, totalCount);
    }
}
