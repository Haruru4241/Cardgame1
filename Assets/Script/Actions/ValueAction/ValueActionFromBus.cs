// Assets/Script/Actions/ValueActionFromBus.cs (새로 생성)

using UnityEngine;

[CreateAssetMenu(fileName = "Value Action from Bus", menuName = "CardGame/Action/Value/Value Action from Bus")]
public class ValueActionFromBus : ValueAction
{
    [Header("참조할 버스 신호")]
    [Tooltip("찾고자 하는 조상 버스의 SignalType 입니다.")]
    public SignalType signalSource;
    [Header("참조할 버스 값")]
    [Tooltip("찾아낸 조상 버스에서 가져올 값의 타입입니다.")]
    public CalcType valueToExtract;
    public override void Execute(SignalBus bus)
    {
        // 1. GetValue()를 통해 최종적으로 사용할 값을 가져옵니다.
        //    (이것이 고정값일수도, TargetSelector의 개수일 수도 있습니다.)
        object finalValue = GetValue(bus);
        if (finalValue == null) return;
        GameManager.Instance._logs += $"밸류{calcType}{op}{finalValue} ";

        // 2. 최종 값을 사용하여 '계산 지시서(Cell)'를 만들어 버스에 추가합니다.
        var cell = new Cell(calcType, op, finalValue, priority);
        bus.AddCalculationStep(cell);
    }
    public override object GetValue(SignalBus bus)
    {
        var ancestorBus = bus.ParentBus; // 부모부터 탐색 시작

        while (ancestorBus != null)
        {
            // 1. 현재 탐색 중인 조상의 SignalType이 우리가 찾는 것과 일치하는지 확인합니다.
            if (ancestorBus.Signal == signalSource)
            {
                // 2. 일치하는 버스를 찾았다면, 거기서 원하는 값을 추출하여 즉시 반환합니다.
                object foundValue = CalculationManager.Instance.Evaluate<object>(ancestorBus, valueToExtract);
                GameManager.Instance._logs += $"[신호값:{foundValue}] ";
                return foundValue;
            }

            // 못 찾았다면, 한 단계 더 윗 조상으로 이동하여 계속 탐색합니다.
            ancestorBus = ancestorBus.ParentBus;
        }

        // 최상위까지 갔는데도 해당 신호를 가진 조상을 못 찾은 경우
        Debug.LogWarning($"ValueFromSignalBusAction: 조상 버스 중에서 '{signalSource}' 신호를 찾지 못했습니다.");
        return null;
    }
}