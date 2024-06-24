using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class CourseRepository : ICourseRepository
{
    private readonly DatabaseConnectionManager connectionManager;

    private CourseDetail lastCourseDetail;

    public V2MigrationStatus MigrationStatus { get; set; }

    public CourseRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public CourseDetail Load(string courseName)
    {
        if (lastCourseDetail?.Name == courseName)
        {
            return lastCourseDetail;
        }
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        List<CourseDto> source = dbConnectionTs.Query<CourseDto>("SELECT * FROM Course WHERE Name=@CourseName AND CachedOn IS NULL LIMIT 1", new
        {
            CourseName = courseName
        }).ToList();
        if (!source.Any())
        {
            return null;
        }
        CourseDto courseDto = source.First();
        lastCourseDetail = BuildCourseFromDto(dbConnectionTs, courseDto);
        return lastCourseDetail;
    }

    public List<CourseDetail> LoadAllDownloaded()
    {
        List<CourseDetail> list = new List<CourseDetail>();
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        foreach (CourseDto item2 in dbConnectionTs.Query<CourseDto>("SELECT * FROM Course WHERE CachedOn IS NULL ORDER BY rowid DESC"))
        {
            CourseDetail item = BuildCourseFromDto(dbConnectionTs, item2);
            list.Add(item);
        }
        return list;
    }

    private CourseDetail BuildCourseFromDto(IDbConnectionTs connection, CourseDto courseDto)
    {
        List<Module> modules = BuildModuleListForCourse(connection, courseDto.Name);
        List<Author> authors = (from name in courseDto.AuthorsFullNames.Split('|')
                                select new Author
                                {
                                    FullName = name
                                }).ToList();
        return new CourseDetail
        {
            Name = courseDto.Name,
            UrlSlug = courseDto.UrlSlug,
            Title = courseDto.Title,
            ReleaseDate = courseDto.ReleaseDate.ParseWithDefault(DateTimeOffset.MinValue),
            UpdatedDate = courseDto.UpdatedDate.ParseWithDefault(DateTimeOffset.MinValue),
            Level = courseDto.Level,
            ShortDescription = courseDto.ShortDescription,
            Description = courseDto.Description,
            DurationInMilliseconds = TimeSpan.FromMilliseconds(courseDto.DurationInMilliseconds),
            HasTranscript = (courseDto.HasTranscript != 0),
            Modules = modules,
            Authors = authors,
            ImageUrl = courseDto.ImageUrl,
            DefaultImageUrl = courseDto.DefaultImageUrl,
            DownloadedOn = courseDto.DownloadedOn.ParseWithDefault(DateTimeOffset.MinValue)
        };
    }

    private List<Module> BuildModuleListForCourse(IDbConnectionTs connection, string courseName)
    {
        IEnumerable<ModuleDto> enumerable = connection.Query<ModuleDto>("SELECT * FROM Module WHERE CourseName=@CourseName ORDER BY ModuleIndex", new
        {
            CourseName = courseName
        });
        List<Module> list = new List<Module>();
        foreach (ModuleDto item2 in enumerable)
        {
            List<Clip> clips = BuildClipListForModule(connection, item2.Id);
            Module item = new Module
            {
                Name = item2.Name,
                Title = item2.Title,
                AuthorHandle = item2.AuthorHandle,
                Description = item2.Description,
                DurationInMilliseconds = TimeSpan.FromMilliseconds(item2.DurationInMilliseconds),
                Clips = clips
            };
            list.Add(item);
        }
        return list;
    }

    private static List<Clip> BuildClipListForModule(IDbConnectionTs connection, int moduleId)
    {
        return (from clipDto in connection.Query<ClipDto>("SELECT * FROM Clip WHERE ModuleId=@ModuleId ORDER BY ClipIndex", new
        {
            ModuleId = moduleId
        })
                select new Clip
                {
                    Name = clipDto.Name,
                    Title = clipDto.Title,
                    Index = clipDto.ClipIndex,
                    DurationInMilliseconds = TimeSpan.FromMilliseconds(clipDto.DurationInMilliseconds),
                    SupportsStandard = (clipDto.SupportsStandard != 0),
                    SupportsWidescreen = (clipDto.SupportsWidescreen != 0)
                }).ToList();
    }

    public void SaveForDownload(CourseDetail course, DateTimeOffset? downloadedOn = null)
    {
        if (!CourseCacheExists(course.Name) || !UpdateCourseForDownload(course.Name, downloadedOn))
        {
            Save(course);
        }
    }

    private bool UpdateCourseForDownload(string courseName, DateTimeOffset? downloadedOn = null)
    {
        if (!downloadedOn.HasValue)
        {
            downloadedOn = DateTimeOffset.UtcNow;
        }
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        return dbConnectionTs.Execute("UPDATE Course SET CachedOn=NULL, DownloadedOn=@downloadedOn WHERE Name=@courseName", new { courseName, downloadedOn }) > 0;
    }

    private bool CourseCacheExists(string courseName)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        return dbConnectionTs.QuerySingle<int>("SELECT COUNT(*) FROM Course  WHERE Name = @courseName", new { courseName }) > 0;
    }

    private void Save(CourseDetail course, DateTimeOffset? cachedOn = null)
    {
        DbConnectionTs connection = connectionManager.Open();
        try
        {
            SQLiteTransaction transaction = connection.BeginTransaction();
            try
            {
                var param = new
                {
                    Name = course.Name,
                    UrlSlug = course.UrlSlug,
                    Title = course.Title,
                    ReleaseDate = course.ReleaseDate.ToDatabaseFormat(),
                    UpdatedDate = course.UpdatedDate.ToDatabaseFormat(),
                    Level = course.Level,
                    ShortDescription = course.ShortDescription,
                    Description = course.Description,
                    DurationInMilliseconds = course.DurationInMilliseconds.TotalMilliseconds,
                    HasTranscript = course.HasTranscript,
                    AuthorsFullNames = string.Join("|", course.Authors.Select((Author a) => a.FullName)),
                    ImageUrl = course.ImageUrl,
                    DefaultImageUrl = course.DefaultImageUrl,
                    CachedOn = cachedOn?.ToDatabaseFormat()
                };
                try
                {
                    connection.Execute("INSERT INTO Course (Name, UrlSlug, Title, ReleaseDate, UpdatedDate, Level, ShortDescription, Description, DurationInMilliseconds, HasTranscript, AuthorsFullNames, ImageUrl, DefaultImageUrl, CachedOn) VALUES (@Name, @UrlSlug, @Title, @ReleaseDate, @UpdatedDate, @Level, @ShortDescription, @Description, @DurationInMilliseconds, @HasTranscript, @AuthorsFullNames, @ImageUrl, @DefaultImageUrl, @CachedOn)", param);
                }
                catch (SQLiteException)
                {
                    return;
                }
                int moduleIndex = 0;
                course.Modules.ForEach(delegate (Module module)
                {
                    var param2 = new
                    {
                        Name = module.Name,
                        Title = module.Title,
                        AuthorHandle = module.AuthorHandle,
                        Description = module.Description,
                        DurationInMilliseconds = module.DurationInMilliseconds.TotalMilliseconds,
                        ModuleIndex = moduleIndex++,
                        CourseName = course.Name
                    };
                    connection.Execute("INSERT INTO Module (Name, Title, AuthorHandle, Description, DurationInMilliseconds, CourseName, ModuleIndex) VALUES (@Name, @Title, @AuthorHandle, @Description, @DurationInMilliseconds, @CourseName, @ModuleIndex)", param2, transaction);
                    long moduleId = connection.ExecuteScalar<long>("SELECT last_insert_rowid()", null, transaction);
                    module.Clips.ForEach(delegate (Clip clip)
                    {
                        var param3 = new
                        {
                            Name = clip.Name,
                            Title = clip.Title,
                            ClipIndex = clip.Index,
                            DurationInMilliseconds = clip.DurationInMilliseconds.TotalMilliseconds,
                            SupportsStandard = clip.SupportsStandard,
                            SupportsWidescreen = clip.SupportsWidescreen,
                            ModuleId = moduleId
                        };
                        connection.Execute("INSERT INTO Clip (Name, Title, ClipIndex, DurationInMilliseconds, SupportsStandard, SupportsWidescreen, ModuleId) VALUES (@Name, @Title, @ClipIndex, @DurationInMilliseconds, @SupportsStandard, @SupportsWidescreen, @ModuleId)", param3);
                    });
                });
                transaction.Commit();
            }
            finally
            {
                if (transaction != null)
                {
                    ((IDisposable)transaction).Dispose();
                }
            }
        }
        finally
        {
            if (connection != null)
            {
                ((IDisposable)connection).Dispose();
            }
        }
    }

    public void Delete(string courseName)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("pragma foreign_keys=on;");
        dbConnectionTs.Execute("DELETE FROM Course WHERE Name=@Name", new
        {
            Name = courseName
        });
    }

    public void ClearAll()
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("DELETE FROM ClipTranscript");
        dbConnectionTs.Execute("DELETE FROM Clip");
        dbConnectionTs.Execute("DELETE FROM Module");
        dbConnectionTs.Execute("DELETE FROM CourseAccess");
        dbConnectionTs.Execute("DELETE FROM Course");
    }

    public CourseDetail LoadFromCache(string courseName)
    {
        using (DbConnectionTs dbConnectionTs = connectionManager.Open())
        {
            List<CourseDto> source = dbConnectionTs.Query<CourseDto>("SELECT * FROM Course WHERE Name=@CourseName LIMIT 1", new
            {
                CourseName = courseName
            }).ToList();
            if (!source.Any())
            {
                return null;
            }
            CourseDto courseDto = source.First();
            DateTimeOffset? dateTimeOffset = courseDto.CachedOn?.ParseWithDefault(DateTimeOffset.MinValue);
            if (!dateTimeOffset.HasValue || (DateTimeOffset.Now - dateTimeOffset.Value).TotalHours < 24.0)
            {
                return BuildCourseFromDto(dbConnectionTs, courseDto);
            }
        }
        Delete(courseName);
        return null;
    }

    public void SaveToCache(CourseDetail course)
    {
        Save(course, DateTimeOffset.Now);
    }
}
