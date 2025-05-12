using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "CardGame/Actions/ApplyToTargets")]
public class ApplyToTargetsAction : CardAction
{
    [System.Serializable]
    public class SignalActionEntry
    {
        public SignalType signal;
        public List<CardAction> actions;
    }
    [Tooltip("대상 추출용 SO")]
    public TargetSelector targetSelector;

    [Tooltip("적용할 Signal별 액션 목록")]
    public List<SignalActionEntry> entries;

    public override void Execute(CardInstance card)
    {
        // 1. 대상 카드 리스트 검색
        var targets = targetSelector.GetTargets(card);

        // 2. 각각의 대상에게 entries 적용
        foreach (var target in targets)
        {
            var proc = new Processor($"복합_타겟_효과_from_{card.CardData.cardName}", false, owner: target, source: card);

            foreach (var entry in entries)
            {
                foreach (var action in entry.actions)
                {
                    proc.Register(entry.signal, action.GetFunction(proc));
                }
            }

            target.AddProcessor(proc);
        }
    }

    public override System.Func<object, object> GetFunction(Processor processor)
    {
        return null; // 외부에서 등록되지 않음
    }
}
