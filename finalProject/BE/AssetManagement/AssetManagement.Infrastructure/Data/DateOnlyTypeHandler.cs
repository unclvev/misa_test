using Dapper;
using System.Data;

namespace AssetManagement.Infrastructure.Data;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.Value = value.ToString("yyyy-MM-dd");
        parameter.DbType = DbType.Date;
    }

    public override DateOnly Parse(object value)
    {
        if (value == null || value == DBNull.Value)
        {
            return default;
        }
        
        if (value is DateTime dateTime)
        {
            return DateOnly.FromDateTime(dateTime);
        }
        
        if (value is DateOnly dateOnly)
        {
            return dateOnly;
        }
        
        if (value is string str && !string.IsNullOrWhiteSpace(str))
        {
            if (DateTime.TryParse(str, out var parsed))
            {
                return DateOnly.FromDateTime(parsed);
            }
        }
        
        // Thử parse trực tiếp từ các kiểu số (timestamp)
        if (value is long longValue)
        {
            return DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeSeconds(longValue).DateTime);
        }
        
        throw new ArgumentException($"Cannot convert {value?.GetType()} ({value}) to DateOnly");
    }
}

