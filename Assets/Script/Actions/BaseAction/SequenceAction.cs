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
        SequenceAction(new SignalBus(driverSignal, bus), heads, tails, new[] { bus.GetSourceCard() });
    }
    // SequenceAction.cs 클래스 내부에 아래 메서드를 추가하세요.

    public override object GetValueForTokenID(string tokenID, SignalBus bus)
    {
        // 나의 하위 액션 목록을 순회합니다.
        foreach (var action in heads)
        {
            // 각 하위 액션에게 "이 ID에 대한 값 아는 사람?" 하고 물어봅니다.
            object value = action.GetValueForTokenID(tokenID, bus);

            // 하위 액션이 값을 찾아서 반환했다면,
            if (value != null)
            {
                // 그 값을 그대로 상위로 전달합니다.
                return value;
            }
        }// 나의 하위 액션 목록을 순회합니다.
        foreach (var action in tails)
        {
            // 각 하위 액션에게 "이 ID에 대한 값 아는 사람?" 하고 물어봅니다.
            object value = action.GetValueForTokenID(tokenID, bus);

            // 하위 액션이 값을 찾아서 반환했다면,
            if (value != null)
            {
                // 그 값을 그대로 상위로 전달합니다.
                return value;
            }
        }
        return null; // 모든 하위 액션을 뒤져도 없으면 null
    }
}
