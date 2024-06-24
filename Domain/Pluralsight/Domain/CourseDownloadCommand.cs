using System.Net;
using System.Threading.Tasks;
using Pluralsight.Domain.Persistance;

namespace Pluralsight.Domain;

public class CourseDownloadCommand
{
    private readonly IUserCourseAccessChecker courseAccessChecker;

    private readonly IRestHelper restHelper;

    private readonly ICourseRepository courseRepository;

    private readonly IDownloadQueue downloadQueue;

    public CourseDownloadCommand(IUserCourseAccessChecker courseAccessChecker, IRestHelper restHelper, ICourseRepository courseRepository, IDownloadQueue downloadQueue)
    {
        this.courseAccessChecker = courseAccessChecker;
        this.restHelper = restHelper;
        this.courseRepository = courseRepository;
        this.downloadQueue = downloadQueue;
    }

    public void SaveForDownload(CourseDetail course)
    {
        courseRepository.SaveForDownload(course);
    }

    public async Task Download(CourseDetail course)
    {
        await downloadQueue.QueueCourseForDownload(course);
    }

    public async Task<CourseDetailResult> GetCourseDetail(string courseName)
    {
        CourseDetail courseDetail = courseRepository.LoadFromCache(courseName);
        if (courseDetail == null)
        {
            RestResponse<CourseDetailDao> restResponse = await restHelper.BasicGet<CourseDetailDao>("/library/courses/" + courseName);
            if (restResponse.StatusCode != HttpStatusCode.OK || restResponse.Data == null)
            {
                return new CourseDetailResult
                {
                    ErrorMessage = "The course '" + courseName + "' could not be found. Please check the URL and try again."
                };
            }
            courseDetail = CourseMapper.ToCourseDetail(restResponse.Data);
            courseRepository.SaveToCache(courseDetail);
        }
        bool? flag = await courseAccessChecker.MayDownload(courseName);
        if (!flag.HasValue || !flag.Value)
        {
            return new CourseDetailResult
            {
                Course = courseDetail,
                ErrorMessage = "Your current subscription does not allow you to download " + courseDetail?.Title + "."
            };
        }
        return new CourseDetailResult
        {
            Course = courseDetail
        };
    }
}
