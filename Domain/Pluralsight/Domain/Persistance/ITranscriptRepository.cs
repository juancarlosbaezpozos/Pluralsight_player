namespace Pluralsight.Domain.Persistance;

public interface ITranscriptRepository
{
	bool SaveTranscriptsForCourse(CourseTranscriptDto courseTranscripts, string courseSlug);

	ClipTranscript[] LoadTranscriptForClip(string courseName, string moduleName, int clipIndex);

	bool TranscriptsAreSaved(string courseName);
}
