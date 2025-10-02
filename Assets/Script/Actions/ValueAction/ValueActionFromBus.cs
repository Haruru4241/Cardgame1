// Assets/Script/Actions/ValueActionFromBus.cs (새로 생성)

using UnityEngine;

[CreateAssetMenu(fileName = "Value Action from Bus", menuName = "CardGame/Action/Value/Value Action from Bus")]
public class ValueActionFromBus : ValueAction
{
    /// <summary>
    /// 인스펙터의 고정값 대신, 현재 버스의 셀에서 값을 읽어와 반환합니다.
    /// </summary>
    public override object GetValue(SignalBus bus)
    {
        Debug.Log($"BusValue{bus.ParentBus.CalcKind}{(int)bus.ParentBus.CalcRaw} ");
        switch (bus.ParentBus.CalcKind)
        {
            case CellKind.Int:
                return (int)bus.ParentBus.CalcRaw;

            case CellKind.Float:
                return (float)bus.ParentBus.CalcRaw;

            case CellKind.String:
                return (string)bus.ParentBus.CalcRaw;

            default:
                return null; // 알 수 없는 타입이면 null 반환
        }
    }
}