using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Newtonsoft.Json;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly DatabaseConnectionManager connectionManager;

    public AnalyticsRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public AnalyticsEvent Load(long rowid)
    {
        return LoadOneRow("SELECT * FROM Analytics WHERE Id=:Id LIMIT 1", new
        {
            Id = rowid
        });
    }

    private AnalyticsEvent LoadOneRow(string sql, object data)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        AnalyticsDto analyticsDto = dbConnectionTs.Query<AnalyticsDto>(sql, data).FirstOrDefault();
        if (analyticsDto == null)
        {
            return null;
        }
        return new AnalyticsEvent
        {
            Id = analyticsDto.Id,
            Properties = JsonConvert.DeserializeObject<IDictionary<string, object>>(analyticsDto.Properties),
            Timestamp = analyticsDto.Timestamp,
            EventName = analyticsDto.EventName,
            FailureCount = analyticsDto.FailureCount
        };
    }

    public long Insert(AnalyticsEvent analyticsEvent)
    {
        long num = 0L;
        AnalyticsDto param = new AnalyticsDto
        {
            EventName = analyticsEvent.EventName,
            Id = analyticsEvent.Id,
            Properties = JsonConvert.SerializeObject(analyticsEvent.Properties),
            Timestamp = analyticsEvent.Timestamp,
            Sent = analyticsEvent.Sent,
            FailureCount = analyticsEvent.FailureCount
        };
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        using SQLiteTransaction sQLiteTransaction = dbConnectionTs.BeginTransaction();
        dbConnectionTs.Execute("INSERT INTO Analytics (EventName, Properties, Timestamp, FailureCount, Sent) VALUES (@EventName, @Properties, @Timestamp, @FailureCount, @Sent)", param, sQLiteTransaction);
        num = dbConnectionTs.ExecuteScalar<long>("SELECT last_insert_rowid()", null, sQLiteTransaction);
        sQLiteTransaction.Commit();
        return num;
    }

    public void Delete(long rowid)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("pragma foreign_keys=on;");
        dbConnectionTs.Execute("DELETE FROM Analytics WHERE Id=@Id", new
        {
            Id = rowid
        });
    }

    public List<AnalyticsEvent> LoadQueue()
    {
        string sql = "SELECT * FROM Analytics WHERE Sent IS NULL OR Sent<'" + DateTimeOffset.Now.AddMinutes(-10.0).ToIso8601Format() + "' LIMIT 100";
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        return dbConnectionTs.Query<AnalyticsDto>(sql).Select(ToAnalyticsEvent).ToList();
    }

    private static AnalyticsEvent ToAnalyticsEvent(AnalyticsDto dto)
    {
        return new AnalyticsEvent
        {
            Id = dto.Id,
            Properties = JsonConvert.DeserializeObject<IDictionary<string, object>>(dto.Properties),
            Timestamp = dto.Timestamp,
            EventName = dto.EventName,
            Sent = dto.Sent,
            FailureCount = dto.FailureCount
        };
    }

    public void DeleteAll()
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("DELETE FROM Analytics");
    }

    public void AddSentTimestamp(long id)
    {
        string sent = DateTimeOffset.Now.ToIso8601Format();
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("UPDATE Analytics SET Sent=@Sent WHERE Id=@Id", new
        {
            Id = id,
            Sent = sent
        });
    }

    public int IncrementFailureCount(long id)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("UPDATE Analytics SET FailureCount = FailureCount + 1 WHERE Id=@Id", new
        {
            Id = id
        });
        return dbConnectionTs.ExecuteScalar<int>("SELECT FailureCount FROM Analytics WHERE Id=@Id", new
        {
            Id = id
        });
    }
}
