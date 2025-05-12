using UnityEngine;
using System;
[CreateAssetMenu(menuName = "CardGame/Actions/SelfRemoveAction")]
public class SelfRemoveProcessorAction : CardAction
{
    public SignalType signal = SignalType.OnTurnEnd;

    public override Func<object, object> GetFunction(Processor processor)
    {
        return _ =>
        {
            processor.SelfDestruct();
            return null;
        };
    }

    public override void Execute(CardInstance card)
    {
        var proc = new Processor("턴 종료 제거", false, card);
        proc.Register(signal, GetFunction(proc));
        card.AddProcessor(proc);
    }
}
