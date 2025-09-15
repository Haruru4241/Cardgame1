using UnityEngine;
using System;

/// <summary>
/// 1) 이 액션이 실행되면 card.Fire(signal)을 호출해서
///    특정 SignalType을 즉시 발동시킵니다.
/// </summary>
[CreateAssetMenu(menuName = "CardGame/Actions/TriggerSignalAction")]
public class TriggerSignalAction : BaseAction
{
    [Tooltip("이 카드에 보낼 시그널")]
    public SignalType signal;
    
    public override void Execute(SignalBus Bus)
    {
        // 즉시 해당 시그널을 카드에 보냅니다.
        Bus.GetSourceCard().Fire(new SignalBus(signal));
    }
}
