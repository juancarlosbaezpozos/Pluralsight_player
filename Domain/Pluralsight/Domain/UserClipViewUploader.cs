using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class UserClipViewUploader
{
	private IRestHelper restHelper;

	private IClipViewRepository clipViewRepository;

	private readonly ICourseRepository courseRepository;

	public UserClipViewUploader(IRestHelper restHelper, IClipViewRepository clipViewRepository, ICourseRepository courseRepository)
	{
		this.restHelper = restHelper;
		this.clipViewRepository = clipViewRepository;
		this.courseRepository = courseRepository;
	}

	public async void UploadAllUnsavedClipViews(AuthenticationToken authenticationToken)
	{
		List<ClipView> clipViews = clipViewRepository.LoadAll();
		List<ViewReport> list = (from x in clipViews.Select(GetViewReport)
			where x != null
			select x).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		ViewReportCollection request = new ViewReportCollection
		{
			Collection = list
		};
		if ((await restHelper.AuthenticatedPost<object, ViewReportCollection>("library/videos/offline/viewreporting", request)).StatusCode == HttpStatusCode.OK)
		{
			clipViewRepository.DeleteSince(clipViews.Max((ClipView x) => x.StartViewTime));
		}
	}

	private ViewReport GetViewReport(ClipView clipView)
	{
		CourseDetail courseDetail = courseRepository.Load(clipView.CourseName);
		if (courseDetail == null)
		{
			return null;
		}
		return new ViewReport
		{
			CourseId = clipView.CourseName,
			ClipId = courseDetail.Modules.First((Module m) => m.Name == clipView.ModuleName).Clips.First((Clip c) => c.Index == clipView.ClipIndex).Name,
			SecondsSinceViewStart = (DateTimeOffset.UtcNow - clipView.StartViewTime).TotalSeconds
		};
	}
}
