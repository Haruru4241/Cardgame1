// Assets/Script/Manager/CalculationManager.cs (이 코드로 교체하세요)

using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System; // Type을 사용하기 위해 추가
using System.Reflection; // ★★★ 리플렉션 사용을 위해 추가 ★★★

[AttributeUsage(AttributeTargets.Field)] // 이 어트리뷰트는 열거형의 필드(멤버)에만 사용할 수 있도록 제한
public class TypeInfoAttribute : Attribute
{
    public Type Type { get; }

    public TypeInfoAttribute(Type type)
    {
        this.Type = type;
    }
}

// ★★★ 이제 CalcType 선언부에 직접 타입을 명시합니다 ★★★
public enum CalcType
{
    [TypeInfo(typeof(int))] DealDamage,
    [TypeInfo(typeof(int))] Health,
    [TypeInfo(typeof(int))] Cost,
    [TypeInfo(typeof(int))] ManaCost,
    [TypeInfo(typeof(string))] Name,
    [TypeInfo(typeof(string))] Description,
    [TypeInfo(typeof(int))] Draw,
    [TypeInfo(typeof(List<BaseInstance>))] TargetList,
    [TypeInfo(typeof(int))] Money,
    [TypeInfo(typeof(int))] TakeDamage,
    // 새로운 타입을 추가할 때, 이 위에 [TypeInfo(typeof(...))] 와 함께 추가하기만 하면 됩니다.
}
public class CalculationManager : MonoBehaviour
{
    public static CalculationManager Instance { get; private set; }

    // 타입별 계산 전문가들을 담아두는 딕셔너리
    private Dictionary<Type, ICalculator> _calculators;
    // ★★★ 핵심 추가: CalcType과 실제 C# 타입을 연결하는 '타입 등록부' ★★★
    public Dictionary<CalcType, Type> _typeRegistry;

    void Awake()
    {
        Instance = this;

        // ★★★ 리플렉션을 이용한 자동 타입 등록 로직 ★★★
        _typeRegistry = new Dictionary<CalcType, Type>();
        var calcTypeEnum = typeof(CalcType);

        // CalcType 열거형의 모든 멤버를 순회합니다.
        foreach (var memberName in Enum.GetNames(calcTypeEnum))
        {
            var memberInfo = calcTypeEnum.GetField(memberName);

            // 멤버 위에 붙어있는 [TypeInfo] 어트리뷰트를 찾습니다.
            var typeInfoAttr = memberInfo.GetCustomAttribute<TypeInfoAttribute>();
            if (typeInfoAttr != null)
            {
                // 어트리뷰트를 찾았다면, 해당 CalcType과 C# 타입을 딕셔너리에 등록합니다.
                CalcType enumValue = (CalcType)Enum.Parse(calcTypeEnum, memberName);
                _typeRegistry.Add(enumValue, typeInfoAttr.Type);
            }
            else
            {
                Debug.LogWarning($"'{memberName}' CalcType에 [TypeInfo] 어트리뷰트가 없어 등록되지 않았습니다.");
            }
        }

        // 타입별 계산 전문가들을 등록합니다.
        _calculators = new Dictionary<Type, ICalculator>
        {
            { typeof(int), new IntCalculator() },
            { typeof(string), new StringCalculator() },
            { typeof(List<BaseInstance>), new InstanceListCalculator() }
            // ...
        };
    }

    /// <summary>
    /// 제네릭을 사용하여 계산 결과를 원하는 타입으로 자동 변환하여 반환합니다.
    /// </summary>
    /// <typeparam name="T">반환받고 싶은 타입</typeparam>
    public T Evaluate<T>(SignalBus bus, CalcType typeToCalculate)
    {
        if (bus == null) return default(T);

        // 1. 타입 등록부에서 이 계산을 수행할 '전문가'의 타입을 가져옵니다.
        if (!_typeRegistry.TryGetValue(typeToCalculate, out Type calculatorType))
        {
            Debug.LogError($"'{typeToCalculate}'에 대한 타입이 CalculationManager에 등록되지 않았습니다!");
            return default(T);
        }

        var steps = bus.CalculationRecipe
            .Where(cell => cell.Type == typeToCalculate)
            .OrderBy(cell => cell.Priority);

        // 2. 해당 타입의 '계산 전문가'를 찾습니다.
        if (_calculators.TryGetValue(calculatorType, out ICalculator calculator))
        {
            // 3. 전문가에게 계산을 위임하고, 결과를 object로 받습니다.
            object calculationResult = calculator.Calculate(steps);

            try
            {
                // 4. ★★★ 핵심 변경점 ★★★
                // 계산 결과를 사용자가 요청한 T 타입으로 변환하여 반환합니다.
                // 예를 들어, int로 계산된 Health 값을 string으로 요청하면 "10" 문자열로 바꿔줍니다.
                return (T)Convert.ChangeType(calculationResult, typeof(T));
            }
            catch (Exception e)
            {
                Debug.LogError($"계산 결과(타입: {calculationResult?.GetType().Name})를 요청된 타입 '{typeof(T).Name}'(으)로 변환할 수 없습니다. 오류: {e.Message}");
                return default(T);
            }
        }

        return default(T);
    }
}

// ICalculator 및 하위 클래스들은 이전과 동일합니다.
// ...
// --- 아래에 계산 전문가 인터페이스와 클래스들을 추가합니다 ---

/// <summary>
/// 모든 타입별 계산 전문가가 따라야 하는 규칙(인터페이스)입니다.
/// </summary>
public interface ICalculator
{
    object Calculate(IEnumerable<Cell> steps);
}

/// <summary>
/// 숫자(int) 계산 전문가입니다.
/// </summary>
public class IntCalculator : ICalculator
{
    public object Calculate(IEnumerable<Cell> steps)
    {
        int result = 0;
        foreach (var step in steps)
        {
            int stepValue = System.Convert.ToInt32(step.Value);
            switch (step.Operation)
            {
                case CalcOp.Set: result = stepValue; break;
                case CalcOp.Add: result += stepValue; break;
                case CalcOp.Sub: result -= stepValue; break;
                    // int에 대한 Mul, Div 등 추가 가능
            }
        }
        return result;
    }
}

/// <summary>
/// 문자열(string) 계산 전문가입니다.
/// </summary>
public class StringCalculator : ICalculator
{
    public object Calculate(IEnumerable<Cell> steps)
    {
        string result = "";
        foreach (var step in steps)
        {
            string stepValue = step.Value?.ToString() ?? "";
            switch (step.Operation)
            {
                // 문자열은 Set(덮어쓰기)과 Add(이어붙이기)만 의미가 있습니다.
                case CalcOp.Set: result = stepValue; break;
                case CalcOp.Add: result += stepValue; break;
            }
        }
        return result;
    }
}

/// <summary>
/// 인스턴스 리스트(List<BaseInstance>) 계산 전문가입니다.
/// </summary>
public class InstanceListCalculator : ICalculator
{
    public object Calculate(IEnumerable<Cell> steps)
    {
        var resultList = new List<BaseInstance>();
        foreach (var step in steps)
        {
            if (step.Value is List<BaseInstance> stepValue)
            {
                switch (step.Operation)
                {
                    case CalcOp.Set: resultList = new List<BaseInstance>(stepValue); break;
                    case CalcOp.Add: resultList.AddRange(stepValue); break;
                        // Sub는 리스트에서 특정 항목들을 제거하는 로직으로 확장 가능
                }
            }
        }
        return resultList.Distinct().ToList();
    }
}

// FloatCalculator 등 다른 전문가들도 위와 같은 방식으로 만들 수 있습니다.
//public class FloatCalculator : ICalculator { /* ... */ }

