using AssetManagement.Infrastructure.Data;
using Dapper;
using System.Data;
using System.Reflection;

namespace AssetManagement.Infrastructure.Repositories;

/// <summary>
/// Base repository class cung cấp các chức năng chung cho tất cả repositories
/// </summary>
public abstract class BaseRepository
{
    protected readonly IDbConnectionFactory ConnectionFactory;

    /// <summary>
    /// Static constructor để đăng ký DateOnly type handler cho Dapper một lần duy nhất
    /// </summary>
    static BaseRepository()
    {
        // Đăng ký DateOnly type handler cho Dapper (chỉ cần đăng ký một lần)
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    }

    /// <summary>
    /// Constructor của BaseRepository
    /// </summary>
    /// <param name="connectionFactory">Factory để tạo database connection</param>
    protected BaseRepository(IDbConnectionFactory connectionFactory)
    {
        ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <summary>
    /// Tạo database connection mới
    /// </summary>
    protected IDbConnection CreateConnection()
    {
        return ConnectionFactory.CreateConnection();
    }

    /// <summary>
    /// Tính toán offset cho phân trang
    /// </summary>
    protected static int CalculateOffset(int pageNumber, int pageSize)
    {
        return (pageNumber - 1) * pageSize;
    }

    /// <summary>
    /// Validate và chuẩn hóa page number và page size
    /// </summary>
    protected static (int PageNumber, int PageSize) ValidatePagination(int pageNumber, int pageSize, int defaultPageSize = 10, int maxPageSize = 100)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = defaultPageSize;
        if (pageSize > maxPageSize) pageSize = maxPageSize;
        return (pageNumber, pageSize);
    }

    /// <summary>
    /// Tạo WHERE clause từ danh sách điều kiện
    /// </summary>
    protected static string BuildWhereClause(List<string> conditions)
    {
        return conditions.Count > 0 
            ? "WHERE " + string.Join(" AND ", conditions)
            : "";
    }

    /// <summary>
    /// Tạo pattern LIKE cho tìm kiếm (thêm % ở đầu và cuối)
    /// </summary>
    protected static string? CreateLikePattern(string? searchText)
    {
        return string.IsNullOrWhiteSpace(searchText) 
            ? null 
            : $"%{searchText.Trim()}%";
    }
}

/// <summary>
/// Generic base repository với CRUD operations cơ bản
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TId">ID type (thường là ulong hoặc int)</typeparam>
public abstract class BaseRepository<TEntity, TId> : BaseRepository where TEntity : class
{
    /// <summary>
    /// Tên bảng trong database (phải được override bởi child class)
    /// </summary>
    protected abstract string TableName { get; }

    /// <summary>
    /// Tên cột ID (mặc định là "id")
    /// </summary>
    protected virtual string IdColumnName => "id";

    /// <summary>
    /// Các cột cần select (có thể override để custom)
    /// </summary>
    protected virtual string SelectColumns => "*";

    /// <summary>
    /// Constructor của BaseRepository
    /// </summary>
    /// <param name="connectionFactory">Factory để tạo database connection</param>
    protected BaseRepository(IDbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    /// <summary>
    /// Lấy entity theo ID
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName}
            WHERE {IdColumnName} = @Id";

        using var connection = CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TEntity>(sql, new { Id = id });
    }

    /// <summary>
    /// Lấy tất cả entities
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName}";

        using var connection = CreateConnection();
        return await connection.QueryAsync<TEntity>(sql);
    }

    /// <summary>
    /// Lấy entities có phân trang
    /// </summary>
    public virtual async Task<(IEnumerable<TEntity> Entities, int TotalCount)> GetAllPagedAsync(int pageNumber, int pageSize, string? orderBy = null)
    {
        var (validPageNumber, validPageSize) = ValidatePagination(pageNumber, pageSize);
        var offset = CalculateOffset(validPageNumber, validPageSize);
        var orderByClause = string.IsNullOrWhiteSpace(orderBy) ? $"ORDER BY {IdColumnName} DESC" : $"ORDER BY {orderBy}";

        var countSql = $"SELECT COUNT(*) FROM {TableName}";
        var sql = $@"
            SELECT {SelectColumns}
            FROM {TableName}
            {orderByClause}
            LIMIT @Offset, @PageSize";

        using var connection = CreateConnection();
        
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
        var entities = await connection.QueryAsync<TEntity>(sql, new { Offset = offset, PageSize = validPageSize });

        return (entities, totalCount);
    }

    /// <summary>
    /// Thêm entity mới (cần override để custom logic)
    /// </summary>
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        // Set CreatedAt và UpdatedAt nếu có
        SetTimestamps(entity, isNew: true);

        // Lấy các properties để build INSERT statement
        var properties = GetInsertableProperties(entity);
        var columnNames = string.Join(", ", properties.Select(p => ToSnakeCase(p.Name)));
        var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        var sql = $@"
            INSERT INTO {TableName} ({columnNames})
            VALUES ({parameterNames});
            SELECT LAST_INSERT_ID();";

        using var connection = CreateConnection();
        
        var id = await connection.ExecuteScalarAsync<TId>(sql, entity);
        
        // Set ID vào entity
        SetId(entity, id);
        
        return entity;
    }

    /// <summary>
    /// Cập nhật entity (cần override để custom logic)
    /// </summary>
    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        // Set UpdatedAt nếu có
        SetTimestamps(entity, isNew: false);

        // Lấy các properties để build UPDATE statement (bỏ qua Id)
        var properties = GetUpdatableProperties(entity);
        var setClause = string.Join(", ", properties.Select(p => $"{ToSnakeCase(p.Name)} = @{p.Name}"));

        var id = GetId(entity);
        var sql = $@"
            UPDATE {TableName}
            SET {setClause}
            WHERE {IdColumnName} = @Id";

        using var connection = CreateConnection();
        await connection.ExecuteAsync(sql, entity);

        return entity;
    }

    /// <summary>
    /// Xóa entity theo ID
    /// </summary>
    public virtual async Task<bool> DeleteAsync(TId id)
    {
        var sql = $"DELETE FROM {TableName} WHERE {IdColumnName} = @Id";

        using var connection = CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        
        return rowsAffected > 0;
    }

    /// <summary>
    /// Kiểm tra entity có tồn tại theo ID không
    /// </summary>
    public virtual async Task<bool> ExistsAsync(TId id)
    {
        var sql = $"SELECT COUNT(*) FROM {TableName} WHERE {IdColumnName} = @Id";

        using var connection = CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
        
        return count > 0;
    }

    /// <summary>
    /// Đếm tổng số records
    /// </summary>
    public virtual async Task<int> CountAsync()
    {
        var sql = $"SELECT COUNT(*) FROM {TableName}";

        using var connection = CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    #region Helper Methods

    /// <summary>
    /// Lấy các properties có thể insert (bỏ qua navigation properties và collections)
    /// </summary>
    protected virtual IEnumerable<PropertyInfo> GetInsertableProperties(TEntity entity)
    {
        return typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !IsNavigationProperty(p))
            .Where(p => p.Name != "Id" || GetId(entity) == null || GetId(entity)!.Equals(default(TId))); // Bỏ qua Id nếu đã có giá trị
    }

    /// <summary>
    /// Lấy các properties có thể update (bỏ qua Id, navigation properties và collections)
    /// </summary>
    protected virtual IEnumerable<PropertyInfo> GetUpdatableProperties(TEntity entity)
    {
        return typeof(TEntity)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !IsNavigationProperty(p))
            .Where(p => p.Name != "Id");
    }

    /// <summary>
    /// Kiểm tra property có phải navigation property không
    /// </summary>
    protected virtual bool IsNavigationProperty(PropertyInfo property)
    {
        var type = property.PropertyType;
        
        // Bỏ qua collections
        if (type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            return true;
        
        // Bỏ qua các type phức tạp (không phải primitive, string, DateTime, DateOnly, decimal)
        if (!type.IsPrimitive && 
            type != typeof(string) && 
            type != typeof(DateTime) && 
            type != typeof(DateTime?) &&
            type != typeof(DateOnly) &&
            type != typeof(DateOnly?) &&
            type != typeof(decimal) &&
            type != typeof(decimal?) &&
            !type.IsValueType)
            return true;
        
        return false;
    }

    /// <summary>
    /// Set CreatedAt và UpdatedAt nếu property tồn tại
    /// </summary>
    protected virtual void SetTimestamps(TEntity entity, bool isNew)
    {
        var now = DateTime.Now;
        var entityType = typeof(TEntity);

        if (isNew)
        {
            var createdAtProp = entityType.GetProperty("CreatedAt");
            if (createdAtProp != null && createdAtProp.CanWrite)
            {
                createdAtProp.SetValue(entity, now);
            }
        }

        var updatedAtProp = entityType.GetProperty("UpdatedAt");
        if (updatedAtProp != null && updatedAtProp.CanWrite)
        {
            updatedAtProp.SetValue(entity, now);
        }
    }

    /// <summary>
    /// Lấy ID từ entity
    /// </summary>
    protected virtual TId? GetId(TEntity entity)
    {
        var idProp = typeof(TEntity).GetProperty("Id");
        if (idProp != null && idProp.CanRead)
        {
            return (TId?)idProp.GetValue(entity);
        }
        return default(TId);
    }

    /// <summary>
    /// Set ID vào entity
    /// </summary>
    protected virtual void SetId(TEntity entity, TId id)
    {
        var idProp = typeof(TEntity).GetProperty("Id");
        if (idProp != null && idProp.CanWrite)
        {
            idProp.SetValue(entity, id);
        }
    }

    /// <summary>
    /// Chuyển PascalCase sang snake_case
    /// </summary>
    protected virtual string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = new System.Text.StringBuilder();
        result.Append(char.ToLowerInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
            {
                result.Append('_');
                result.Append(char.ToLowerInvariant(input[i]));
            }
            else
            {
                result.Append(input[i]);
            }
        }

        return result.ToString();
    }

    #endregion
}
