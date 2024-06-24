namespace Pluralsight.Domain;

public class AutoplayStrategyFactory
{
    public static AutoplayStrategy GetStrategy(AutoplaySetting setting)
    {
        return setting switch
        {
            AutoplaySetting.Off => new NoAutoplayStrategy(),
            AutoplaySetting.Module => new ModuleAutoplayStrategy(),
            _ => new CourseAutoplayStrategy(),
        };
    }
}
