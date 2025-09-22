using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Actions/SelectDestroyAction")]
public class SelectDestroyAction : BaseAction
{
    public SignalType triggerSignal = SignalType.OnEffect;
    public int requiredCount = 1;
    public string processorName = "SelectAndDestroy";

    public override void Execute(SignalBus Bus)
    {
        var candidates = DeckManager.Instance.GetPile(PileType.Hand).Cards.ToList();
        var selectState = GameManager.Instance.SelectState as SelectState;

        selectState.StartSelection(
            () => DeckManager.Instance.GetPile(PileType.Hand).Cards.ToList(),
            requiredCount,
            selectedList =>
            OnSelectionFinished(selectedList, Bus),
            Bus // 🔹 버블 토큰 관리 위해 현재 버스 전달
        );
    }
    private void OnSelectionFinished(List<BaseInstance> list, SignalBus bus)
    {
        var busesToPush = new List<SignalBus>();

        foreach (var ci in list)
        {
            busesToPush.Add(ci.PrepareBus(new SignalBus(SignalType.OnDestroy, bus)));

        }

        // 3. 모든 준비가 끝난 후, 한 번에 출발시킵니다.
        if (busesToPush.Count > 0)
            ReactionStackManager.Instance.PushBuses(busesToPush); // PushSequence 사용 권장
    }
}
