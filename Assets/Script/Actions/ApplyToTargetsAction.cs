using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "CardGame/Actions/ApplyToTargets")]
public class ApplyToTargetsAction : BaseAction
{
    [System.Serializable]
    public class ActionEntry
    {
        public SignalType signal;
        public List<BaseAction> actions;
    }

    [Tooltip("대상 추출용 SO")]
    public TargetSelector targetSelector;

    [Tooltip("적용할 Signal별 액션 목록")]
    public List<ActionEntry> entries;

    /// <summary>
    /// 이 액션이 실행되면, 지정한 대상을 찾아
    /// 각 대상에 대해 entries에 정의된 CardAction을
    /// 대응되는 신호에 등록해주는 프로세서를 붙입니다.
    /// </summary>
    public override void Execute(SignalBus bus)
    {
        var origin = bus.GetSourceCard();
        if (origin == null) return;

        var targets = targetSelector.GetTargets(origin);
        foreach (var target in targets)
        {
            // 1) 대상별 새 프로세서 생성
            var proc = new Processor(
                sourceName: $"Apply_",
                isBase:      false,
                owner:       target,
                source:      origin
            );

            // 2) 각 SignalActionEntry의 signal에, 그 리스트의 CardAction을 등록
            foreach (var entry in entries)
            {
                foreach (var action in entry.actions)
                {
                    // 람다 대신 CardAction 자체를 붙입니다
                    proc.RegisterAction(entry.signal, action);
                }
            }

            // 3) 대상 카드 인스턴스에 프로세서 추가
            target.AddProcessor(proc);
        }
    }
}