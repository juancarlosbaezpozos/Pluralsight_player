using System.Linq;

namespace Pluralsight.Domain;

public abstract class AutoplayStrategy
{
    public abstract NextClipModel ClipFinished(CourseDetail course, Module currentModule, Clip currentClip);

    protected bool IsLastClip(CourseDetail course, Module currentModule, Clip currentClip)
    {
        if (course.Modules.Last() == currentModule)
        {
            return currentModule.Clips.Last() == currentClip;
        }
        return false;
    }

    protected NextClipModel CourseCompleteModel()
    {
        return new NextClipModel
        {
            OverlayTitle = "Course complete!",
            ContinueText = "Return to your downloaded courses"
        };
    }
}
