using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using Dapper;

namespace Pluralsight.Domain.WPF.Persistance;

public class DbConnectionTs : IDbConnectionTs, IDisposable
{
    public SQLiteConnection Connection;

    private static readonly SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

    public DbConnectionTs()
    {
        Lock.Wait();
    }

    public void Dispose()
    {
        Connection?.Dispose();
        Lock.Release();
    }

    public IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null)
    {
        return Connection.Query<T>(sql, param, transaction);
    }

    public T QueryFirstOrDefault<T>(string sql, object param = null, IDbTransaction transaction = null)
    {
        return Connection.QueryFirstOrDefault<T>(sql, param, transaction);
    }

    public int Execute(string sql, object param = null, IDbTransaction transaction = null)
    {
        return Connection.Execute(sql, param, transaction);
    }

    public T QuerySingle<T>(string sql, object param = null, IDbTransaction transaction = null)
    {
        return Connection.QuerySingle<T>(sql, param);
    }

    public T ExecuteScalar<T>(string sql, object param = null, IDbTransaction transaction = null)
    {
        return Connection.ExecuteScalar<T>(sql, param, transaction);
    }

    public SQLiteTransaction BeginTransaction()
    {
        return Connection.BeginTransaction();
    }

    public T QuerySingleOrDefault<T>(string sql)
    {
        return Connection.QuerySingleOrDefault<T>(sql);
    }
}
