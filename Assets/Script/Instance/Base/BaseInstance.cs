using System.Collections.Generic;
using System;                  // Action 델리게이트를 위해
using System.Linq;             // ToList() 확장 메서드를 위해
using UnityEngine;

public abstract class BaseInstance
{
    public List<Processor> _processors = new();
    public BaseController controller { get; set; }
    public Zone CurrentZone { get; set; }

    public BaseData _data { get; protected set; }

    public void AddProcessor(Processor p)
    {
        // 핵심 로직을 재사용합니다.
        AddProcessors(new[] { p });
    }
    public void AddProcessors(IEnumerable<Processor> processorsToAdd)
    {
        foreach (var p in processorsToAdd)
        {
            if (p != null)
            {
                _processors.Add(p);
            }
        }

        controller?.UpdateUI();
    }
    public void RemoveProcessor(Processor processor)
    {
        // 핵심 로직을 재사용합니다.
        RemoveProcessors(new[] { processor });
    }
    public void RemoveProcessors(IEnumerable<Processor> processorsToRemove)
    {
        foreach (var p in processorsToRemove)
        {
            if (_processors.Contains(p))
            {
                _processors.Remove(p);
                controller?.UpdateUI();
            }
        }

        controller?.UpdateUI();
    }
    public void RegisterProcessor(SignalType signal, List<BaseAction> actions)
    {
        if (actions == null || actions.Count == 0) return;

        var processor = new Processor($"CardData_{signal}", isBase: false, owner: this, source: this);

        foreach (var action in actions)
        {
            processor.RegisterAction(signal, action);
        }

        AddProcessor(processor);
    }
    public IEnumerable<Processor> GetProcessorsFor(SignalBus bus)
    {
        return _processors.Where(p => p.GetActionsFor(bus.Signal).Any());
    }

    // 2) CreateBaseProcessor → getter 함수만 넘기면 SO를 만들어 등록
    protected Processor CreateBaseProcessorAction(
    SignalType signal,
    object value,           // int/float/string/SignalType 모두 가능
    CalcOp op = CalcOp.Set  // Set / Add / Sub
)
    {
        var action = ScriptableObject
            .CreateInstance<ValueAction>()
            .Initialize(op, value);

        var proc = new Processor($"Base_{signal}", isBase: true, owner: this, source: this);
        proc.RegisterAction(signal, action);
        return proc;
    }
    public List<ActionBubble> BuildBubblesForSignal(SignalBus bus)
    {
        var reactingProcs = GetProcessorsFor(bus).ToList();
        var bubbles = new List<ActionBubble>(reactingProcs.Count);

        foreach (var p in reactingProcs)
        {
            var q = p.BuildActionQueue(bus.Signal);
            if (q != null && q.Count > 0)
                bubbles.Add(new ActionBubble(q, p));
        }

        return bubbles;
    }
    public SignalBus PrepareBus(SignalBus bus)
    {
        var Bubbles = BuildBubblesForSignal(bus);
        if (Bubbles.Count == 0) return bus; // 실행할 액션이 없으면 null 반환

        bus.AddPassengers(Bubbles);
        bus.SetSourceInfo(this);

        return bus;
    }



    public abstract void Fire(SignalBus bus);
}
