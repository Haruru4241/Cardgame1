using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/CostReducerAction")]
public class CostReducerAction : CardAction
{
    public int delta = -1;

    public SignalType signal = SignalType.OnTurnEnd;

    public override Func<object, object> GetFunction(Processor processor)
    {
        return input => (int)input + delta;
    }

    public override void Execute(CardInstance card)
    {
        // 개별 실행 시 직접 프로세서 생성도 가능
        var proc = new Processor("코스트 감소", false, card);
        proc.Register(signal, GetFunction(proc));
        card.AddProcessor(proc);
    }
}
