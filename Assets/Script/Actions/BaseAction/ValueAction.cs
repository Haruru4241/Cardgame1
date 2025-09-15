using UnityEngine;

// 메뉴 이름과 파일 이름을 최종 버전인 ValueAction으로 통일합니다.
[CreateAssetMenu(menuName = "CardGame/Actions/ValueAction")]
public class ValueAction : BaseAction
{
    // 사용할 값의 타입을 정의하는 열거형
    public enum ValueType
    {
        Int,
        Float,
        String,
        Bool
    }
    [Header("실행 우선순위 (낮을수록 먼저 실행)")]
    public int priority = 3;

    [Header("계산 연산")]
    [Tooltip("Set / Add / Sub 등 Calc에서 처리할 연산자입니다.")]
    public CalcOp op = CalcOp.Set;

    [Header("값")]
    [Tooltip("사용할 값의 타입을 선택하세요.")]
    public ValueType valueType; // 인스펙터에서 타입을 선택할 Enum

    // 각 타입에 해당하는 값을 저장할 변수들
    public int intValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;

    /// <summary>
    /// 인스펙터에서 설정된 valueType에 따라 실제 값을 object 형태로 반환합니다.
    /// </summary>
    public object Value
    {
        get
        {
            switch (valueType)
            {
                case ValueType.Int:    return intValue;
                case ValueType.Float:  return floatValue;
                case ValueType.String: return stringValue;
                case ValueType.Bool:   return boolValue;
                default:               return null;
            }
        }
    }

    /// <summary>
    /// [핵심 추가] 코드에서 동적으로 생성하고 초기화할 때 사용합니다.
    /// 전달받은 value의 타입에 따라 valueType과 실제 값을 설정합니다.
    /// </summary>
    public ValueAction Initialize(CalcOp op, object value)
    {
        this.op = op;

        // value의 실제 타입을 확인하고 그에 맞게 값을 설정
        if (value is int intVal)
        {
            this.valueType = ValueType.Int;
            this.intValue = intVal;
        }
        else if (value is float floatVal)
        {
            this.valueType = ValueType.Float;
            this.floatValue = floatVal;
        }
        else if (value is string stringVal)
        {
            this.valueType = ValueType.String;
            this.stringValue = stringVal;
        }
        else if (value is bool boolVal)
        {
            this.valueType = ValueType.Bool;
            this.boolValue = boolVal;
        }
        else
        {
            // 지원하지 않는 타입이 들어올 경우 경고
            if (value != null)
            {
                Debug.LogWarning($"ValueAction.Initialize: 지원하지 않는 타입({value.GetType()})의 값이 전달되었습니다.");
            }
        }
        
        return this;
    }

    /// <summary>
    /// 버스가 지정된 신호일 때만 Calc를 1회 수행합니다.
    /// </summary>
    public override void Execute(SignalBus bus)
    {
        if (bus == null) return;
        GameManager.Instance._logs += $"밸류 {Value}, {op} ";
        // Value 프로퍼티를 통해 현재 타입에 맞는 값을 전달합니다.
        bus.Calc(op, Value);
        
    }
}