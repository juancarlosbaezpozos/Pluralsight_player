namespace Pluralsight.Domain;

public class ClipDownloadInfo
{
    public CourseDetail Course;

    public Module Module;

    public Clip Clip;

    public int RetryCount;

    public bool IsFirstClipInCourse
    {
        get
        {
            if (Clip.Index == 0)
            {
                return Course.Modules[0] == Module;
            }
            return false;
        }
    }
}
