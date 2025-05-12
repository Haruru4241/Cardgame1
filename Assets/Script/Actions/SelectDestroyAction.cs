using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Actions/SelectDestroyAction")]
public class SelectDestroyAction : CardAction
{
    [Tooltip("이 액션이 반응할 시그널")]
    public SignalType triggerSignal = SignalType.OnEffect;

    [Tooltip("선택해야 할 카드 개수")]
    public int requiredCount = 1;

    [Tooltip("프로세서 이름")]
    public string processorName = "SelectAndDestroy";

    public override void Execute(CardInstance card)
    {
        GetFunction(null)?.Invoke(null);
    }

    public override Func<object, object> GetFunction(Processor processor)
    {
        return _ =>
        {
            var handCards = DeckManager.Instance.HandPile.Cards;
            var candidates = handCards.ToList();  // List<CardInstance>

            // 2) SelectState 호출
            var selectState = GameManager.Instance.SelectState as SelectState;
            selectState.StartSelection(
                candidates,
                requiredCount,
                selectedList =>
                {
                    // 3) 선택된 카드들에 OnDestroy 신호 발사
                    foreach (var ci in selectedList)
                        ci.Fire(SignalType.OnDestroy);
                }
            );
            return null;
        };
    }
}
