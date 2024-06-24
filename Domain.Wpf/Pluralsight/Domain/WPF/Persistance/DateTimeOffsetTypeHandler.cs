using System;
using System.Data;
using Dapper;

namespace Pluralsight.Domain.WPF.Persistance;

public class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.Value = value.ToString();
    }

    public override DateTimeOffset Parse(object value)
    {
        if (value == null || value is DBNull)
        {
            return DateTimeOffset.MinValue;
        }
        return DateTimeOffset.Parse(value.ToString());
    }
}
