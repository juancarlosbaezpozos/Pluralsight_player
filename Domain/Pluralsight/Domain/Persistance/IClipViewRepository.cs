using System;
using System.Collections.Generic;

namespace Pluralsight.Domain.Persistance;

public interface IClipViewRepository
{
	List<ClipView> LoadAll();

	void Save(ClipView clipView);

	void DeleteSince(DateTimeOffset startTime);

	void Migrate(ClipView clipViewV1, string courseId, string moduleId);
}
