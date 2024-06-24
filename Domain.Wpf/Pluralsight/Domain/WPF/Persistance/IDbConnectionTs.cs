using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Pluralsight.Domain.WPF.Persistance;

public interface IDbConnectionTs
{
	IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null);

	T QueryFirstOrDefault<T>(string sql, object param = null, IDbTransaction transaction = null);

	int Execute(string sql, object param = null, IDbTransaction transaction = null);

	T QuerySingle<T>(string sql, object param = null, IDbTransaction transaction = null);

	T ExecuteScalar<T>(string sql, object param = null, IDbTransaction transaction = null);

	T QuerySingleOrDefault<T>(string sql);

	SQLiteTransaction BeginTransaction();
}
