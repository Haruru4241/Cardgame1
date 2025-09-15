using System.Collections.Generic;
using System;                  // Action 델리게이트를 위해
using System.Linq;             // ToList() 확장 메서드를 위해
using UnityEngine;

public abstract class BaseInstance
{
    public List<Processor> _processors = new();
    public BaseCard BaseCard { get; set; }
    public Zone CurrentZone { get; set; }

    public BaseData BaseData { get; protected set; }


    public void AddProcessor(Processor p)
    {
        _processors.Add(p);
        BaseCard?.UpdateUI();
    }
    public void RemoveProcessor(Processor processor)
    {
        if (_processors.Contains(processor))
        {
            _processors.Remove(processor);
            BaseCard?.UpdateUI();
        }
    }
    public void RegisterProcessorAction(SignalType signal, List<BaseAction> actions)
    {
        if (actions == null || actions.Count == 0) return;

        // 기존과 동일하게 프로세서 생성
        var processor = new Processor($"CardData_{signal}", isBase: false, owner: this, source: this);

        foreach (var action in actions)
        {
            // 람다 대신 액션 객체 자체를 저장
            processor.RegisterAction(signal, action);
        }

        AddProcessor(processor);
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
    public object Evaluate(SignalBus bus)
    {
        if (bus == null) return null;

        // 이전 계산 리셋
        bus.CalcReset();

        // 이 신호에 등록된 액션들의 Execute만 실행 (계산 전용 액션만 등록되어야 함)
        foreach (var proc in _processors)
            foreach (var action in proc.GetActionsFor(bus.Signal))
                action.Execute(bus);

        // 최종 결과: 셀의 Raw 값 반환 (타입은 bus.CalcKind로 구분)
        return bus.CalcRaw;
    }
    public void EvalRun(SignalBus Bus, IEnumerable<BaseAction> headActions = null, IEnumerable<BaseAction> tailActions = null)
    {
        if (Bus == null) return;

        // 1) 평가용 Bus 생성 + 소스 승계
        var evalBus = Bus;

        if (headActions != null)
        {
            var q = new Queue<BaseAction>();
            foreach (var a in headActions)
                if (a != null) q.Enqueue(a);

            if (q.Count > 0)
                evalBus.AddPassengers(new[] { new ActionBubble(q) });
        }

        // 3) Source 기준으로 해당 신호의 '계산' 버블 수집 후 탑승
        var source = Bus.GetSourceCard();
        if (source != null)
        {
            var calcBubbles = source.BuildBubblesForSignal(evalBus);
            if (calcBubbles != null && calcBubbles.Count > 0)
                evalBus.AddPassengers(calcBubbles);
        }

        // 4) tailActions를 하나의 큐에 순서대로 담아 '단일 버블'로 추가
        if (tailActions != null)
        {
            var q = new Queue<BaseAction>();
            foreach (var a in tailActions)
                if (a != null) q.Enqueue(a);

            if (q.Count > 0)
                evalBus.AddPassengers(new[] { new ActionBubble(q) });
        }

        // 5) 실행
        ReactionStackManager.Instance.PushBus(evalBus);
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



    public abstract void Fire(SignalBus bus);
}
