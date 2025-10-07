using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class BaseAction : ScriptableObject
{
    [Header("Description Settings")]
    [Tooltip("설명 텍스트의 키워드와 연결됩니다. (예: Damage, Draw, Freeze)")]
    public string DescriptionKey;
    // 즉시 실행
    public abstract void Execute(SignalBus bus);

    public void SequenceAction(SignalBus Bus, IEnumerable<BaseAction> headActions = null, IEnumerable<BaseAction> tailActions = null, IEnumerable<BaseInstance> Targets = null)
    {
        if (Bus == null) return;

        // 1) 평가용 Bus 생성 + 소스 승계
        var evalBus = Bus;
        evalBus.SetSourceInfo(Bus.ParentBus?.GetSourceCard());

        if (headActions != null)
        {
            var q = new Queue<BaseAction>();
            foreach (var a in headActions)
                if (a != null) q.Enqueue(a);

            if (q.Count > 0)
                evalBus.AddPassengers(new[] { new ActionBubble(q) });
        }

        if (Targets != null)
        {
            foreach (var target in Targets)
            {
                var calcBubbles = target.BuildBubblesForSignal(evalBus);
                if (calcBubbles != null && calcBubbles.Count > 0)
                    evalBus.AddPassengers(calcBubbles);
            }
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

    /// <summary>
    /// 이 액션의 기본 수치 값을 반환합니다. 툴팁의 기본값을 표시할 때 사용됩니다.
    /// 자식 클래스에서 이 메서드를 재정의(override)해야 합니다.
    /// </summary>
    public virtual object GetDefaultValue()
    {
        return null;
    }
}
