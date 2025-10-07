using UnityEngine;
using System;

/// <summary>
/// 1) 이 액션이 실행되면 card.Fire(signal)을 호출해서
///    특정 SignalType을 즉시 발동시킵니다.
/// </summary>
[CreateAssetMenu(menuName = "CardGame/Actions/TriggerSignalAction")]
public class TriggerSignalAction : BaseAction
{
    public TargetSelector targetSelector;
    [Tooltip("이 카드에 보낼 시그널")]
    public SignalType signal;

    public override void Execute(SignalBus Bus)
    {
        // 1. TargetSelector를 이용해 모든 대상을 찾습니다.
        var targets = targetSelector.GetTargets(Bus.GetSourceCard(), Bus);

        foreach (var target in targets)
        {
            GameManager.Instance._logs += $"타겟{target._data.Name} 트리거{signal} ";
            target.Fire(new SignalBus(signal, Bus));
        }
    }
}
