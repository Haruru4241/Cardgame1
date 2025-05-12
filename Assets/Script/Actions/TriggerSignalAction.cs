using UnityEngine;
using System;

/// <summary>
/// 1) 이 액션이 실행되면 card.Fire(signal)을 호출해서
///    특정 SignalType을 즉시 발동시킵니다.
/// </summary>
[CreateAssetMenu(menuName = "CardGame/Actions/TriggerSignalAction")]
public class TriggerSignalAction : CardAction
{
    [Tooltip("이 카드에 보낼 시그널")]
    public SignalType signal;
    
    public override Func<object, object> GetFunction(Processor processor)
    {
        return _ =>
        {
            processor.Owner?.Fire(signal);
            return null;
        };
    }

    public override void Execute(CardInstance card)
    {
        // 즉시 해당 시그널을 카드에 보냅니다.
        card.Fire(signal);
    }
}
