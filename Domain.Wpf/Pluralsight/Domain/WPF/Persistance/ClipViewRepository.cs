using System;
using System.Collections.Generic;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class ClipViewRepository : IClipViewRepository
{
    private readonly DatabaseConnectionManager connectionManager;

    public ClipViewRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public List<ClipView> LoadAll()
    {
        List<ClipView> list = new List<ClipView>();
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        foreach (ClipViewDto item2 in dbConnectionTs.Query<ClipViewDto>("SELECT * FROM ClipView"))
        {
            ClipView item = new ClipView
            {
                CourseName = item2.CourseName,
                AuthorHandle = item2.AuthorHandle,
                ModuleName = item2.ModuleName,
                ClipIndex = item2.ClipIndex,
                StartViewTime = item2.StartViewTime.ParseWithDefault(DateTimeOffset.UtcNow)
            };
            list.Add(item);
        }
        return list;
    }

    public void Save(ClipView clipView)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("INSERT INTO ClipView (CourseName, AuthorHandle, ModuleName, ClipIndex, StartViewTime) VALUES (@CourseName, @AuthorHandle, @ModuleName, @ClipIndex, @StartViewTime)", new
        {
            CourseName = clipView.CourseName,
            AuthorHandle = clipView.AuthorHandle,
            ModuleName = clipView.ModuleName,
            ClipIndex = clipView.ClipIndex,
            StartViewTime = clipView.StartViewTime.ToDatabaseFormat()
        });
    }

    public void Migrate(ClipView clipViewV1, string courseId, string moduleId)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("UPDATE ClipView SET CourseName=@CourseName, ModuleName=@ModuleName WHERE CourseName=@CourseSlug AND ModuleName=@ModuleSlug AND AuthorHandle=@AuthorHandle", new
        {
            CourseName = courseId,
            AuthorHandle = clipViewV1.AuthorHandle,
            ModuleName = moduleId,
            ClipIndex = clipViewV1.ClipIndex,
            StartViewTime = clipViewV1.StartViewTime.ToDatabaseFormat(),
            ModuleSlug = clipViewV1.ModuleName,
            CourseSlug = clipViewV1.CourseName
        });
    }

    public void DeleteSince(DateTimeOffset startTime)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("DELETE FROM ClipView WHERE StartViewTime <= @StartViewTime", new
        {
            StartViewTime = startTime.ToDatabaseFormat()
        });
    }
}
