using System.Collections.Generic;
using System.Linq;

public class ArtifactInstance : BaseInstance
{
    public ArtifactData Data;

    public ArtifactInstance(ArtifactData data)
    {
        Data = data;
        RegisterProcessor(SignalType.OnDraw, data.onDrawActions);
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
