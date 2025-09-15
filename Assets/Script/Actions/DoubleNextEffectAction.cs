using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Actions/DoubleNextEffectAction")]
public class DoubleNextEffectAction : BaseAction
{
    public SignalType triggerSignal;
    public string processorName = "RepeatNextEffect";
    public int repeatCount = 2;
    public int requiredCount = 1;

    public override void Execute(SignalBus Bus)
    {
        var candidates = DeckManager.Instance.GetPile(PileType.Hand).Cards.ToList();
        var selectState = GameManager.Instance.SelectState as SelectState;

        selectState.StartSelection(
            candidates,
            Mathf.Min(requiredCount, candidates.Count),
            list => OnSelectionFinished(list, Bus),
            Bus // 🔹 현재 버스 전달
        );
    }
    private void OnSelectionFinished(List<BaseInstance> list, SignalBus Bus)
    {
        var busesToPush = new List<SignalBus>();

        foreach (var ci in list)
        {
            // 선택된 카드(ci)의 프로세서 중 triggerSignal을 가진 것만 추출
            var originals = ci._processors
                .Where(p => p.GetActionsFor(triggerSignal).Any())
                .ToList();

            if (originals.Count == 0)
                continue;

            var bubbles = new List<ActionBubble>();

            for (int i = 0; i < repeatCount; i++)
            {
                foreach (var p in originals)
                {
                    // 🔹 한 번만 큐 생성
                    var q = p.BuildActionQueue(triggerSignal);

                    bubbles.Add(new ActionBubble(q));
                }
            }

            // 🔹 카드별로 독립 Bus 생성
            var bus = new SignalBus(triggerSignal, Bus);
            bus.SetSourceInfo(ci);   // 실행 주체: 선택된 카드 b
            bus.AddPassengers(bubbles);

            busesToPush.Add(bus);
        }
        if (busesToPush.Count > 0)
            ReactionStackManager.Instance.PushBuses(busesToPush);
    }
}
