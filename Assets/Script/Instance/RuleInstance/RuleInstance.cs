using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 룰 전용 런타임 인스턴스.
/// - processors: 기존 카드가 갖는 Processor와 동일 인터페이스를 갖는 항목들을 추가 가능
/// - extraActions: 신호별로 임의의 BaseAction들을 붙여둘 수 있음 (예: 드로우 5장)
/// </summary>
public class RuleInstance : BaseInstance
{
    // 기존 카드의 프로세서와 동일한 타입을 가정
    public List<Processor> processors = new List<Processor>();

    // 신호별로 룰 액션을 직접 추가할 수 있게 (턴 시작/종료 같은 고정 룰)
    private readonly Dictionary<SignalType, List<BaseAction>> extraActions = new();

    public RuleInstance()
    {
    }


    public override void Fire(SignalBus bus)
    {
        // 버스에 탑승시키고 처리 시작
        bus.AddPassengers(BuildBubblesForSignal(bus));
        bus.SetSourceInfo(this);
        ReactionStackManager.Instance.PushBus(bus);
    }
}
