using System.Collections.Generic;
using System.Linq;

public class ArtifactInstance : BaseInstance
{
    public ArtifactData Data;

    public ArtifactInstance(ArtifactData data)
    {
        Data = data;
        foreach (var entry in data.actionEntries)
        {
            // 엔트리에 액션이 하나라도 있을 경우에만 등록
            if (entry.actions != null && entry.actions.Count > 0)
            {
                RegisterProcessor(entry.signal, entry.actions);
            }
        }
    }

    public IEnumerable<ActionBubble> GetConditionalPassengers(SignalBus bus)
    {
        foreach (var p in _processors)
        {
            var q = p.BuildActionQueue(bus.Signal);
            if (q.Count > 0) yield return new ActionBubble(q);
        }
    }

    public override void Fire(SignalBus bus)
    {
        var passengers = GetConditionalPassengers(bus).ToList();
        bus.AddPassengers(passengers);
    }
}
