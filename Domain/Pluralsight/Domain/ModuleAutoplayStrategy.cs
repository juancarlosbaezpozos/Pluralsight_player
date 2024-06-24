namespace Pluralsight.Domain;

public class ModuleAutoplayStrategy : AutoplayStrategy
{
	public override NextClipModel ClipFinished(CourseDetail course, Module currentModule, Clip currentClip)
	{
		if (IsLastClip(course, currentModule, currentClip))
		{
			return CourseCompleteModel();
		}
		int num = currentModule.Clips.IndexOf(currentClip);
		if (num + 1 < currentModule.Clips.Count)
		{
			return new NextClipModel
			{
				NextModule = currentModule,
				NextClip = currentModule.Clips[num + 1]
			};
		}
		return new NextClipModel
		{
			OverlayTitle = "Module complete!",
			ContinueText = "Continue to next module"
		};
	}
}
