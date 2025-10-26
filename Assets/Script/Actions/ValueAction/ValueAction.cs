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
        //GameManager.Instance._logs += $"밸류{op}{finalValue} ";
        EventManager.Instance.LogEvent(LogType.Value, $"밸류{op}{finalValue}", bus.Signal, null, null, bus);
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
    public override object GetValueForTokenID(string tokenID, SignalBus bus)
    {
        // 나의 엔트리 목록에 요청된 ID가 있는지 확인합니다.
        foreach (var entry in descriptionEntries)
        {
            if (entry.TokenID == tokenID)
            {
                // 있다면, 나의 GetValue() 결과를 반환합니다.
                return this.GetValue(bus);
            }
        }
        return null; // 없으면 null
    }
}