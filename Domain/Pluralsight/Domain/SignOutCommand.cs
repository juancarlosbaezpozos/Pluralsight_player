using System;
using Pluralsight.Domain.Authentication;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class SignOutCommand
{
	private readonly IRestHelper restHelper;

	private readonly IUserRepository userRepository;

	private readonly IClipViewRepository clipViewRepository;

	private readonly IUserProfileRepository userProfileRepository;

	private readonly ICourseProgressRepository courseProgressRepository;

	private readonly ICourseRepository courseRepository;

	private readonly ICourseAccessRepository courseAccessRepository;

	private readonly IDownloadQueue downloadQueue;

	public SignOutCommand(IRestHelper restHelper, IUserRepository userRepository, IClipViewRepository clipViewRepository, IUserProfileRepository userProfileRepository, ICourseProgressRepository courseProgressRepository, ICourseRepository courseRepository, ICourseAccessRepository courseAccessRepository, IDownloadQueue downloadQueue)
	{
		this.restHelper = restHelper;
		this.userRepository = userRepository;
		this.clipViewRepository = clipViewRepository;
		this.userProfileRepository = userProfileRepository;
		this.courseProgressRepository = courseProgressRepository;
		this.courseRepository = courseRepository;
		this.courseAccessRepository = courseAccessRepository;
		this.downloadQueue = downloadQueue;
	}

	public void Execute(User userToSignOut)
	{
		restHelper.AuthenticatedDelete<string>("user/device/" + userToSignOut.DeviceInfo.DeviceId);
		userRepository.Delete();
		clipViewRepository.DeleteSince(DateTimeOffset.UtcNow);
		userProfileRepository.Delete();
		courseProgressRepository.ClearAll();
		foreach (CourseDetail item in courseRepository.LoadAllDownloaded())
		{
			downloadQueue.DeleteCourse(item);
		}
		courseRepository.ClearAll();
		courseAccessRepository.ClearAll();
	}
}
