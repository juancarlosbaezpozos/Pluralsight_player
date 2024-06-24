namespace Pluralsight.Domain;

public class NoAutoplayStrategy : AutoplayStrategy
{
	public override NextClipModel ClipFinished(CourseDetail course, Module currentModule, Clip currentClip)
	{
		if (IsLastClip(course, currentModule, currentClip))
		{
			return CourseCompleteModel();
		}
		return new NextClipModel
		{
			NextClip = null,
			NextModule = null,
			OverlayTitle = "Clip complete!",
			ContinueText = "Continue to next clip"
		};
	}
}
