using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Action/Select and Destroy")]
public class SelectAndDestroyAction2 : BaseAction
{
    [Header("1. 어떤 대상을?")]
    public TargetSelector targetSelector;

    [Header("2. 어떤 방식으로?")]
    public SelectionMode selectionMode;

    [Header("3. 몇 개를?")]
    public int requiredCount = 1;

    public override void Execute(SignalBus bus)
    {
        var interactionState = GameManager.Instance.InteractionState as InteractionState;

        // 후보 수량 보정은 InteractionState.Enter()에서 처리하므로 여기서 미리 계산할 필요가 없습니다.

        interactionState.StartSelection(
            selectionMode,
            () => DeckManager.Instance.GetPile(ZoneType.Hand).Cards.ToList(),
            requiredCount,
            selectedList => OnSelectionFinished(selectedList, bus),
            bus
        );
    }
    private void OnSelectionFinished(List<BaseInstance> list, SignalBus bus)
    {
        var busesToPush = new List<SignalBus>();
        //GameManager.Instance._logs += "대상 ";
        foreach (var ci in list)
        {
            //GameManager.Instance._logs += $"{ci._data.Name} ";
            busesToPush.Add(ci.PrepareBus(new SignalBus(SignalType.OnDestroy, bus)));
        }
        EventManager.Instance.LogEvent(
        LogType.Target, // (가정) 대상을 지정하는 로그
        $"{string.Join(", ", list.Select(instance => instance._data.Name))}",
        bus.Signal,          // 이 로직을 유발한 원본 신호
        bus.GetSourceCard(), // (가정) 원본 신호의 Source
        list,                // (가정) 타겟이 여러 명이라 특정 타겟을 지정하지 않음
        bus                  // 전체 버스 문맥
    );
        // 3. 모든 준비가 끝난 후, 한 번에 출발시킵니다.
        if (busesToPush.Count > 0)
            ReactionStackManager.Instance.PushBuses(busesToPush); // PushSequence 사용 권장
    }
}