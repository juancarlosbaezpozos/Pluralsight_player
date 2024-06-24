using System;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain.WPF.Persistance;

public class CourseAccessRepository : ICourseAccessRepository
{
    private readonly DatabaseConnectionManager connectionManager;

    public CourseAccessRepository(DatabaseConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
    }

    public bool? Load(string courseName)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        CourseAccessDto courseAccessDto = dbConnectionTs.QueryFirstOrDefault<CourseAccessDto>("SELECT * FROM CourseAccess WHERE CourseName=@CourseName", new
        {
            CourseName = courseName
        });
        if (courseAccessDto == null)
        {
            return null;
        }
        return courseAccessDto.MayDownload != 0;
    }

    public void Save(string courseName, bool mayDownload)
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        var param = new
        {
            CourseName = courseName,
            MayDownload = mayDownload,
            LastChecked = DateTimeOffset.UtcNow.ToDatabaseFormat()
        };
        if (dbConnectionTs.Execute("UPDATE CourseAccess SET MayDownload=@MayDownload, LastChecked=@LastChecked WHERE CourseName=@CourseName", param) == 0)
        {
            dbConnectionTs.Execute("INSERT INTO CourseAccess (CourseName, MayDownload, LastChecked) VALUES (@CourseName, @MayDownload, @LastChecked)", param);
        }
    }

    public void ClearAll()
    {
        using DbConnectionTs dbConnectionTs = connectionManager.Open();
        dbConnectionTs.Execute("DELETE FROM CourseAccess");
    }
}
