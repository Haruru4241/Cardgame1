using UnityEngine;
using System;
using System.Collections.Generic;

//복합적인 효과를 가진 새로운 프로세서를 추가하는 액션

[CreateAssetMenu(menuName = "CardGame/Actions/CompositeProcessorAction")]
public class CompositeProcessorAction : BaseAction
{
    [System.Serializable]
    public class SignalActionEntry
    {
        [Tooltip("어떤 신호에 붙일지")]
        public SignalType signal;

        [Tooltip("해당 신호에 실행할 액션들")]
        public List<BaseAction> actions;
    }

    [Tooltip("생성될 프로세서 이름")]
    public string processorName = "복합 효과";

    [Tooltip("신호별로 묶어둘 액션 리스트")]
    public List<SignalActionEntry> entries;

    /// <summary>
    /// 이 액션이 발동될 때, 하나의 Processor를 생성해서
    /// entries에 정의된 모든 신호·액션 매핑을 등록합니다.
    /// </summary>
    public override void Execute(SignalBus bus)
    {
        var card = bus.GetSourceCard();
        if (card == null) return;

        // 1) 새 프로세서 생성
        var proc = new Processor(
            sourceName: processorName,
            isBase:      false,
            owner:       card,
            source:      card
        );

        // 2) entries에 정의된 대로 CardAction 자체를 붙이기
        foreach (var entry in entries)
        {
            foreach (var action in entry.actions)
            {
                proc.RegisterAction(entry.signal, action);
            }
        }

        // 3) 카드에 프로세서 추가
        card.AddProcessor(proc);
    }
}