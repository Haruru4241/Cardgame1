// Assets/Script/Actions/BaseAction/ValueAction.cs (이 코드로 교체하세요)

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System; // Type을 사용하기 위해 추가

[CreateAssetMenu(fileName = "Value Action", menuName = "CardGame/Action/Value/Value Action")]
public class ValueAction : BaseAction
{
    [Tooltip("이 지시서가 어떤 종류의 값에 대한 것인지")]
    public CalcType calcType;

    [Tooltip("수행할 연산")]
    public CalcOp op = CalcOp.Set;

    [Tooltip("계산 우선순위")]
    public int priority = 3;

    // --- 인스펙터에서 설정할 다양한 타입의 고정 값들 ---
    public int intValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;
    public UnityEngine.Object objectValue; // BaseInstance, CardData 등 Unity Object 참조

    // ---

    /// <summary>
    /// 이 액션이 지시서에 적을 최종 값을 결정합니다.
    /// 자식 클래스(Provider)에서 이 메서드를 재정의하여 동적인 값을 제공할 수 있습니다.
    /// </summary>
    public virtual object GetValue(SignalBus bus)
    {
        // CalculationManager에 등록된 정보(typeRegistry)를 바탕으로
        // 이 calcType이 어떤 C# 타입이어야 하는지 확인합니다.
        if (CalculationManager.Instance != null &&
            CalculationManager.Instance._typeRegistry.TryGetValue(this.calcType, out Type expectedType))
        {
            // 타입에 맞는 값을 정확히 반환합니다.
            if (expectedType == typeof(string))
                return this.stringValue;
            if (expectedType == typeof(int))
                return this.intValue;
            if (expectedType == typeof(float))
                return this.floatValue;
            if (expectedType == typeof(bool))
                return this.boolValue;
            // List<BaseInstance> 와 같은 참조 타입들을 처리합니다.
            if (expectedType.IsSubclassOf(typeof(UnityEngine.Object)) || expectedType == typeof(List<BaseInstance>))
                return this.objectValue;
        }

        // 만약 CalculationManager에 등록되지 않은 타입이라면, 경고를 남기고 int를 기본으로 사용합니다.
        Debug.LogWarning($"'{calcType}'에 대한 타입을 찾을 수 없어 intValue를 기본값으로 사용합니다.");
        return this.intValue;
    }

    public override void Execute(SignalBus bus)
    {
        // 1. GetValue()를 통해 최종적으로 사용할 값을 가져옵니다.
        //    (이것이 고정값일수도, TargetSelector의 개수일 수도 있습니다.)
        object finalValue = GetValue(bus);
        if (finalValue == null) return;
        GameManager.Instance._logs += $"밸류{op}{finalValue} ";

        // 2. 최종 값을 사용하여 '계산 지시서(Cell)'를 만들어 버스에 추가합니다.
        var cell = new Cell(calcType, op, finalValue, priority);
        bus.AddCalculationStep(cell);
    }

    /// <summary>
    /// 코드에서 동적으로 생성된 ValueAction을 초기화하는 안전한 방법입니다.
    /// </summary>
    public void Initialize(CalcOp op, object value, CalcType type)
    {
        this.op = op;
        this.calcType = type;

        if (value is int i) this.intValue = i;
        else if (value is float f) this.floatValue = f;
        else if (value is string s) this.stringValue = s;
        else if (value is bool b) this.boolValue = b;
        else if (value is UnityEngine.Object o) this.objectValue = o;
    }
}
// using UnityEngine;

// // 메뉴 이름과 파일 이름을 최종 버전인 ValueAction으로 통일합니다.
// [CreateAssetMenu(menuName = "CardGame/Actions/ValueAction")]
// public class ValueAction : BaseAction
// {
//     // 사용할 값의 타입을 정의하는 열거형
//     public enum ValueType
//     {
//         Int,
//         Float,
//         String,
//         Bool
//     }
//     [Tooltip("이 지시서가 어떤 종류의 값에 대한 것인지")]
//     public CalcType calcType;

//     [Header("실행 우선순위 (낮을수록 먼저 실행)")]
//     public int priority = 3;

//     [Header("계산 연산")]
//     [Tooltip("Set / Add / Sub 등 Calc에서 처리할 연산자입니다.")]
//     public CalcOp op = CalcOp.Set;

//     [Header("값")]
//     [Tooltip("사용할 값의 타입을 선택하세요.")]
//     public ValueType valueType; // 인스펙터에서 타입을 선택할 Enum

//     [Tooltip("값을 읽어올 셀의 타입입니다. (예: Damage)")]
//     public CalcType valueSourceType;

//     // 각 타입에 해당하는 값을 저장할 변수들
//     public int intValue;
//     public float floatValue;
//     public string stringValue;
//     public bool boolValue;

//     /// <summary>
//     /// 인스펙터에서 설정된 valueType에 따라 실제 값을 object 형태로 반환합니다.
//     /// </summary>
//     public object Value
//     {
//         get
//         {
//             switch (valueType)
//             {
//                 case ValueType.Int: return intValue;
//                 case ValueType.Float: return floatValue;
//                 case ValueType.String: return stringValue;
//                 case ValueType.Bool: return boolValue;
//                 default: return null;
//             }
//         }
//     }

//     /// <summary>
//     /// [핵심 추가] 코드에서 동적으로 생성하고 초기화할 때 사용합니다.
//     /// 전달받은 value의 타입에 따라 valueType과 실제 값을 설정합니다.
//     /// </summary>
//     public ValueAction Initialize(CalcOp op, object value, CalcType type)
//     {
//         this.op = op;
//         this.calcType = type;

//         // value의 실제 타입을 확인하고 그에 맞게 값을 설정
//         if (value is int intVal)
//         {
//             this.valueType = ValueType.Int;
//             this.intValue = intVal;
//         }
//         else if (value is float floatVal)
//         {
//             this.valueType = ValueType.Float;
//             this.floatValue = floatVal;
//         }
//         else if (value is string stringVal)
//         {
//             this.valueType = ValueType.String;
//             this.stringValue = stringVal;
//         }
//         else if (value is bool boolVal)
//         {
//             this.valueType = ValueType.Bool;
//             this.boolValue = boolVal;
//         }
//         else
//         {
//             // 지원하지 않는 타입이 들어올 경우 경고
//             if (value != null)
//             {
//                 Debug.LogWarning($"ValueAction.Initialize: 지원하지 않는 타입({value.GetType()})의 값이 전달되었습니다.");
//             }
//         }

//         return this;
//     }

//     /// <summary>
//     /// 버스가 지정된 신호일 때만 Calc를 1회 수행합니다.
//     /// </summary>
//     public virtual object GetValue(SignalBus bus)
//     {
//         // 기본적으로는 인스펙터에 설정된 고정값을 반환합니다.
//         return this.Value;
//     }

//     public override void Execute(SignalBus bus)
//     {
//         bus.AddCalculationStep(new Cell(calcType, op, GetValue(bus), priority));
//     }
// }