using Dapper;

namespace Pluralsight.Domain.WPF.Persistance;

public class DapperConfig
{
    static DapperConfig()
    {
        SqlMapper.ResetTypeHandlers();
        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
    }
}
