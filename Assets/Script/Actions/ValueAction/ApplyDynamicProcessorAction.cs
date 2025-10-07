// Assets/Script/Actions/ApplyDynamicProcessorAction.cs (이 코드로 교체하세요)

using UnityEngine;

[CreateAssetMenu(fileName = "Apply Dynamic Processor Action", menuName = "CardGame/Action/Apply Dynamic Processor")]
public class ApplyDynamicProcessorAction : BaseAction
{
    [Header("1. 누구에게?")]
    [Tooltip("이 동적 프로세서를 추가할 대상을 지정합니다.")]
    public TargetSelector targetSelector;

    [Header("2. 어떤 프로세서를?")]
    [Tooltip("새로운 프로세서가 반응할 신호")]
    public SignalType signalToReact;
    [Tooltip("값을 계산할 때 사용할 연산")]
    public CalcOp operation;
    [Tooltip("새로운 ValueAction이 최종적으로 처리할 값의 타입 (예: Health)")]
    public CalcType valueTypeToProcess;

    [Header("3. 어떤 값으로?")]
    [Tooltip("프로세서가 사용할 값(Value)을 어디서 가져올지 지정합니다. (예: TakeDamage)")]
    public CalcType valueSourceType;

    public override void Execute(SignalBus bus)
    {
        var origin = bus.GetSourceCard(); // 이 액션을 실행하는 주체
        GameManager.Instance._logs += $"({origin}) ";
        if (origin == null) return;

        // ★★★ 1. TargetSelector를 이용해 대상을 찾습니다. ★★★
        //    - 기준점(origin)은 이 액션을 실행한 카드로 전달합니다.
        var targets = targetSelector.GetTargets(origin, bus);

        if (targets == null || targets.Count == 0) return;
        
        // 1. 현재 버스에서 동적인 값을 가져옵니다. (TakeDamageEvaluation의 최종 피해량 등)
        object valueFromBus = CalculationManager.Instance.Evaluate<object>(bus, valueSourceType);

        // ★★★ 2. 찾은 모든 대상에게 동적 프로세서를 추가합니다. ★★★
        foreach (var target in targets)
        {
            if (target == null) continue;

            // 2. 가져온 값을 사용하는 새로운 ValueAction을 메모리상에 동적으로 생성합니다.
            var dynamicValueAction = ScriptableObject.CreateInstance<ValueAction>();
            dynamicValueAction.Initialize(operation, valueFromBus, valueTypeToProcess);

            // 3. 생성된 ValueAction을 담을 새로운 프로세서를 만듭니다.
            var newProcessor = new Processor(
                sourceName: $"DynamicEffect_{signalToReact}",
                isBase: false,
                owner: target,
                source: origin
            );
            newProcessor.RegisterAction(signalToReact, dynamicValueAction);

            // 4. 대상에게 완성된 프로세서를 추가합니다.
            target.AddProcessor(newProcessor);
            
            GameManager.Instance._logs += $"({target._data.name})에게 동적Pro추가 ";
        }
    }
}