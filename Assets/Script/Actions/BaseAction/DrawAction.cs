using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[CreateAssetMenu(menuName = "CardGame/Actions/DrawAction")]
public class DrawAction : BaseAction
{
    public override void Execute(SignalBus bus)
    {
        var ci = DeckManager.Instance.DrawOne();
        if (ci == null || ci.BaseCard == null) return;
        GameManager.Instance._logs += $"드로우1 ";

        // ② 뽑힌 카드의 OnDraw 신호
        var childBus = new SignalBus(SignalType.OnDraw, bus);
        childBus.SetSourceInfo(bus.GetSourceCard());
        ci.BaseCard.cardInstance.Fire(childBus);
    }
}