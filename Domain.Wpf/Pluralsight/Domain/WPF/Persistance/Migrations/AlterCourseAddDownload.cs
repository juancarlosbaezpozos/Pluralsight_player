using System;
using System.Collections.Generic;
using System.Data;

namespace Pluralsight.Domain.WPF.Persistance.Migrations;

internal class AlterCourseAddDownload : IMigration
{
	public int VersionNumber => 15;

	public void ApplyMigration(DatabaseConnectionManager connectionManager)
	{
		using IDbConnection dbConnection = connectionManager.OpenConnection();
		using (IDbCommand dbCommand = dbConnection.CreateCommand())
		{
			dbCommand.CommandText = "ALTER TABLE Course ADD COLUMN DownloadedOn text";
			dbCommand.ExecuteNonQuery();
		}
		CourseRepository courseRepository = new CourseRepository(connectionManager);
		List<CourseDetail> list = courseRepository.LoadAllDownloaded();
		for (int i = 0; i < list.Count; i++)
		{
			CourseDetail courseDetail = list[i];
			if (courseDetail.DownloadedOn == DateTimeOffset.MinValue)
			{
				courseRepository.SaveForDownload(courseDetail, DateTimeOffset.UtcNow.AddDays(-7.0).AddHours(-1 * i));
			}
		}
	}
}
