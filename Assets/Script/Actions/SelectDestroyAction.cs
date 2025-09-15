using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Actions/SelectDestroyAction")]
public class SelectDestroyAction : BaseAction
{
    public SignalType triggerSignal = SignalType.OnEffect;
    public int        requiredCount = 1;
    public string     processorName = "SelectAndDestroy";

    public override void Execute(SignalBus Bus)
    {
        var candidates  = DeckManager.Instance.GetPile(PileType.Hand).Cards.ToList();
            var selectState = GameManager.Instance.SelectState as SelectState;

            selectState.StartSelection(
                candidates,
                Mathf.Min(requiredCount, candidates.Count),
                selectedList =>
                {
                    foreach (var ci in selectedList)
                        ci.Fire(new SignalBus(SignalType.OnDestroy));
                },
                Bus // 🔹 버블 토큰 관리 위해 현재 버스 전달
            );
    }
}
