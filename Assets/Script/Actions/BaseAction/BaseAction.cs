using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseAction : ScriptableObject
{
    [Header("동적 설명 엔트리")]
    [Tooltip("이 액션이 설명문의 특정 토큰({ID})에 값을 제공할 경우, 여기에 연결 정보를 정의합니다.")]
    public List<DescriptionEntry> descriptionEntries;
    // 즉시 실행
    public abstract void Execute(SignalBus bus);

    public void SequenceAction(SignalBus Bus, IEnumerable<BaseAction> headActions = null, IEnumerable<BaseAction> tailActions = null, IEnumerable<BaseInstance> Targets = null)
    {
        if (Bus == null) return;

        // 1) 평가용 Bus 생성 + 소스 승계
        var evalBus = Bus;
        evalBus.SetSourceInfo(Bus.ParentBus?.GetSourceCard());

        EventManager.Instance.LogEvent(LogType.ActionExecuting, $"SequenceAction{headActions.Count()} {tailActions.Count()}", Bus.Signal, null, null, Bus);

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
    public virtual object GetValueForTokenID(string tokenID, SignalBus bus)
    {
        // 기본적으로는 아무 값도 제공하지 않습니다.
        return null;
    }
}
