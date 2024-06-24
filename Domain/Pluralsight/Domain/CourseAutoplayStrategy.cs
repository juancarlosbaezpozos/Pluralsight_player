namespace Pluralsight.Domain;

public class CourseAutoplayStrategy : AutoplayStrategy
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
        int num2 = course.Modules.IndexOf(currentModule);
        Module module = course.Modules[num2 + 1];
        return new NextClipModel
        {
            NextModule = module,
            NextClip = module.Clips[0]
        };
    }
}
