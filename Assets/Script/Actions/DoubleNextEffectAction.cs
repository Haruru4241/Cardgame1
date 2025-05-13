using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Actions/DoubleNextEffectAction")]
public class DoubleNextEffectAction : CardAction
{
    [Tooltip("이 액션이 복제할 대상 신호")]
    public SignalType triggerSignal;

    [Tooltip("인스펙터에 표시될 프로세서 이름")]
    public string processorName = "RepeatNextEffect";

    [Tooltip("현재 효과 뒤에 몇 번 더 반복할지")]
    public int repeatCount = 2;

    public override void Execute(CardInstance card, Processor processor)
    {
        GetFunction(processor)?.Invoke(null);
    }

    public override Func<object, object> GetFunction(Processor processor)
    {
        return _ =>
        {
            // 선택 모드 진입: HandPile 기준으로 선택
            var candidates = DeckManager.Instance.HandPile.Cards.ToList();
            var selState = GameManager.Instance.SelectState as SelectState;
            selState.StartSelection(candidates, 1, processor, list =>
            {
                var batch = new List<Processor>();
                foreach (var ci in list)
                {
                    // OnEffect 신호를 처리하는 프로세서 필터링
                    var original = ci.processors
                        .Where(p => p.GetHandlersFor(triggerSignal).Any())
                        .ToList();

                    if (original.Count == 0)
                        continue;

                    // repeatCount만큼 복제하여 하나의 배치로 구성
                    for (int i = 0; i < repeatCount; i++)
                    {
                        batch.AddRange(original);
                    }
                }

                ReactionStackManager.Instance.PushReactions(SignalType.OnEffect, batch);
            });

            return null;
        };
    }
}
