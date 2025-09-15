using UnityEngine;
using System;
[CreateAssetMenu(menuName = "CardGame/Actions/SelfRemoveAction")]
public class SelfRemoveProcessorAction : BaseAction
{
    public SignalType signal = SignalType.OnTurnEnd;

    public override void Execute(SignalBus Bus)
    {
        Bus.FrontBubble.OwnerProcessor.SelfDestruct();
    }
}
