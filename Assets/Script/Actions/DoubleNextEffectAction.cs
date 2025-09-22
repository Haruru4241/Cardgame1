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
            ()=>DeckManager.Instance.GetPile(PileType.Hand).Cards.ToList(),
            requiredCount,
            list => OnSelectionFinished(list, Bus),
            Bus // 🔹 현재 버스 전달
        );
    }
    private void OnSelectionFinished(List<BaseInstance> list, SignalBus Bus)
    {
        var busesToPush = new List<SignalBus>();

        foreach (var ci in list)
        {
            for (int i = 0; i < repeatCount; i++)
            {
                busesToPush.Add(ci.PrepareBus(new SignalBus(triggerSignal, Bus)));
            }
        }

        // 3. 모든 준비가 끝난 후, 한 번에 출발시킵니다.
        if (busesToPush.Count > 0)
            ReactionStackManager.Instance.PushBuses(busesToPush); // PushSequence 사용 권장
    }
}
