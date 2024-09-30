namespace DependencyAnalysis.Utils;

public class HashSetHelper
{
    public static void SetAddRange<T>(HashSet<T> set, IEnumerable<T> range)
    {
        foreach (var thing in range)
        {
            set.Add(thing);
        }
    }
}