using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class CourseProgressRepository : ICourseProgressRepository
{
    private readonly DatabaseConnectionManager connectionManager;

    public CourseProgressRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public CourseProgress Load(string courseName)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        CourseProgressDto courseProgressDto = dbConnectionTs.QueryFirstOrDefault<CourseProgressDto>("SELECT * FROM CourseProgress WHERE Name=@Name", new
        {
            Name = courseName
        });
        if (courseProgressDto == null)
        {
            return null;
        }
        return new CourseProgress
        {
            CourseName = courseProgressDto.Name,
            ViewedModules = JsonConvert.DeserializeObject<List<ModuleProgress>>(courseProgressDto.ViewedModules),
            LastViewed = ((courseProgressDto.LastViewTime == null) ? null : new LastViewedClipInfo
            {
                ModuleName = courseProgressDto.LastViewedModuleName,
                ModuleAuthorHandle = courseProgressDto.LastViewedModuleAuthor,
                ClipModuleIndex = courseProgressDto.LastViewedClip,
                ViewTime = courseProgressDto.LastViewTime.ParseWithDefault(DateTimeOffset.MinValue)
            })
        };
    }

    public void Save(CourseProgress courseProgress)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        var param = new
        {
            Name = courseProgress.CourseName,
            ViewedModules = JsonConvert.SerializeObject(courseProgress.ViewedModules),
            LastViewedModuleName = courseProgress.LastViewed?.ModuleName,
            LastViewedModuleAuthor = courseProgress.LastViewed?.ModuleAuthorHandle,
            LastViewedClip = courseProgress.LastViewed?.ClipModuleIndex,
            LastViewTime = courseProgress.LastViewed?.ViewTime.ToDatabaseFormat()
        };
        if (dbConnectionTs.Execute("UPDATE CourseProgress SET ViewedModules=@ViewedModules, LastViewedModuleName=@LastViewedModuleName, LastViewedModuleAuthor=@LastViewedModuleAuthor, LastViewedClip=@LastViewedClip, LastViewTime=@LastViewTime WHERE Name=@Name", param) == 0)
        {
            dbConnectionTs.Execute("INSERT INTO CourseProgress (Name, ViewedModules, LastViewedModuleName, LastViewedModuleAuthor, LastViewedClip, LastViewTime) VALUES (@Name, @ViewedModules, @LastViewedModuleName, @LastViewedModuleAuthor, @LastViewedClip, @LastViewTime)", param);
        }
    }

    public void Delete(string courseName)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("DELETE FROM CourseProgress WHERE Name=@Name", new
        {
            Name = courseName
        });
    }

    public void ClearAll()
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("DELETE FROM CourseProgress");
    }
}
