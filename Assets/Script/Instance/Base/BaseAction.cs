using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class BaseAction : ScriptableObject
{
    // 즉시 실행
    public abstract void Execute(SignalBus bus);

    protected void EvalRun(SignalBus Bus, IEnumerable<BaseAction> headActions, IEnumerable<BaseAction> tailActions, IEnumerable<BaseInstance> Targets = null)
    {
        if (Bus == null) return;

        // 1) 평가용 Bus 생성 + 소스 승계
        var evalBus = Bus;
        evalBus.SetSourceInfo(Bus.ParentBus.GetSourceCard());

        if (headActions != null)
        {
            var q = new Queue<BaseAction>();
            foreach (var a in headActions)
                if (a != null) q.Enqueue(a);

            if (q.Count > 0)
                evalBus.AddPassengers(new[] { new ActionBubble(q) });
        }
        
        foreach (var target in Targets)
        {
            var calcBubbles = target.BuildBubblesForSignal(evalBus);
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
        evalBus.SortBubblesByPriority();

        // 5) 실행
        ReactionStackManager.Instance.PushBus(evalBus);
    }
}
