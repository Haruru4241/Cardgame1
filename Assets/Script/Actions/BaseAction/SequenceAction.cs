using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Actions/SequenceAction")]
public class SequenceAction : BaseAction
{
    public SignalType driverSignal = SignalType.OnDrawDriver;
    public BaseAction[] heads;

    // 예: 셀 최종 Int를 사용해 드로우하는 액션을 꼬리에 붙인다
    public BaseAction[] tails; // 인스펙터에서 DrawCardsFromCellAction 등 지정

    public override void Execute(SignalBus bus)
    {
        EvalRun(new SignalBus(driverSignal, bus), heads, tails, new[] { bus.GetSourceCard() });
    }
}
