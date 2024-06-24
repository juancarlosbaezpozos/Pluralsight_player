using System.Linq;

namespace Pluralsight.Domain;

public static class CourseTranscriptMapper
{
    public static CourseTranscriptDto ToCourseTranscriptDto(CourseTranscriptDao dao, CourseDetail course)
    {
        return new CourseTranscriptDto
        {
            CourseName = dao.CourseId,
            Modules = dao.Modules.Select((ModuleTranscriptDao m) => ToModuleTranscriptDto(m, course)).ToList()
        };
    }

    private static ModuleTranscriptDto ToModuleTranscriptDto(ModuleTranscriptDao dao, CourseDetail course)
    {
        return new ModuleTranscriptDto
        {
            ModuleName = dao.ModuleId,
            ClipTranscripts = dao.ClipTranscripts.Select((ClipTranscriptDao c) => ToClipTranscriptDto(c, dao.ModuleId, course)).ToList()
        };
    }

    private static ClipTranscriptDto ToClipTranscriptDto(ClipTranscriptDao dao, string moduleId, CourseDetail course)
    {
        return new ClipTranscriptDto
        {
            ModuleIndexPosition = course.GetClipIndexFromIds(moduleId, dao.ClipId),
            Transcripts = dao.Transcripts.Select(ToTranscriptsDto).ToList()
        };
    }

    private static TranscriptDto ToTranscriptsDto(TranscriptDao dao)
    {
        return new TranscriptDto
        {
            RelativeEndTime = dao.RelativeEndTime,
            RelativeStartTime = dao.RelativeStartTime,
            Text = dao.Text
        };
    }
}
