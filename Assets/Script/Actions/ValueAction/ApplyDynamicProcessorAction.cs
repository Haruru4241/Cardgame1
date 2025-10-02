// Assets/Script/Actions/ApplyDynamicProcessorAction.cs 경로에 새로 생성하세요.

using UnityEngine;

[CreateAssetMenu(fileName = "Apply Dynamic Processor Action", menuName = "CardGame/Action/Apply Dynamic Processor")]
public class ApplyDynamicProcessorAction : BaseAction
{
    [Header("새로운 프로세서 설정")]
    [Tooltip("이 프로세서가 반응할 신호 타입입니다.")]
    public SignalType signalToReact; // 여기서는 'HPEvaluation'을 지정

    [Tooltip("프로세서에 담길 ValueAction의 연산 방식입니다.")]
    public CalcOp operation; // 여기서는 'Sub'(빼기)를 지정

    public override void Execute(SignalBus bus)
    {
        Debug.Log($"ApplyProcessor{bus.CalcRaw} ");
        var source = bus.GetSourceCard(); // 이 액션을 실행하는 주체 (피해를 입는 자신)
        var target = source; // 이 효과는 자기 자신에게 적용됩니다.

        if (target == null) return;

        // 1. 현재 버스에서 동적인 값을 가져옵니다. (TakeDamageEvaluation의 최종 피해량)
        int dynamicValue = (int)bus.CalcRaw;

        // 2. 가져온 값을 사용하는 새로운 ValueAction을 메모리상에 동적으로 생성합니다.
        var dynamicValueAction = ScriptableObject.CreateInstance<ValueAction>();
        dynamicValueAction.Initialize(operation, dynamicValue);

        // 3. 생성된 ValueAction을 담을 새로운 프로세서를 만듭니다.
        var newProcessor = new Processor(
            sourceName: $"DynamicEffect_{signalToReact}",
            isBase: false,
            owner: target,
            source: source
        );
        newProcessor.RegisterAction(signalToReact, dynamicValueAction);

        // 4. 대상(자기 자신)에게 완성된 프로세서를 추가합니다.
        target.AddProcessor(newProcessor);
    }
}