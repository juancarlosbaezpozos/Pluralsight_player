using System;
using System.Collections.Generic;

namespace Pluralsight.Domain.Persistance;

public interface ICourseRepository
{
	V2MigrationStatus MigrationStatus { get; set; }

	List<CourseDetail> LoadAllDownloaded();

	CourseDetail Load(string courseName);

	void SaveForDownload(CourseDetail course, DateTimeOffset? downloadedOn = null);

	void Delete(string courseName);

	void ClearAll();

	CourseDetail LoadFromCache(string courseName);

	void SaveToCache(CourseDetail course);
}
