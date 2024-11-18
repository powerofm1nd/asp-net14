using System.Diagnostics;
using OpenTelemetry;

namespace asp_net_13;

public class ActivityFilteringProcessor : BaseProcessor<Activity>
{
    public override void OnStart(Activity activity)
    {
        string? priority = activity.Baggage.FirstOrDefault(b => b.Key == "priority").Value;

        if (priority == "high")
        {
            activity.SetTag("priority", "high");
        }
        
        if (!activity.Tags.Any(tag => tag.Key == "priority" && tag.Value == "high"))
        {
            activity.IsAllDataRequested = false;
        }
    }
}