using System.Collections.Generic;
using System;                  // Action 델리게이트를 위해
using System.Linq;             // ToList() 확장 메서드를 위해
using UnityEngine;
using Unity.VisualScripting;
public class CardInstance : BaseInstance
{
    public CardInstance(CardData data)
    {
        BaseData = data;

        CurrentZone = null;
        SetupBaseProcessors(data);
    }
    // 3) 초기값 프로세서 등록 예시
    public void SetupBaseProcessors(CardData data)
    {
        GameManager.Instance._logs += $"인스턴스 셋업 - ";
        // 이름
        AddProcessor(CreateBaseProcessorAction(
            SignalType.NameEvaluation,
            data.Name,            // string
            CalcOp.Set));

        // 설명
        AddProcessor(CreateBaseProcessorAction(
            SignalType.DescriptionEvaluation,
            data.Description,     // string
            CalcOp.Set));

        // 마나 코스트
        AddProcessor(CreateBaseProcessorAction(
            SignalType.ManaCostEvaluation,
            data.manaCost,        // int
            CalcOp.Set));

        // 구매 비용
        AddProcessor(CreateBaseProcessorAction(
            SignalType.BuyCostEvaluation,
            data.Cost,            // int
            CalcOp.Set));

        // 아트워크 (Sprite)  ← 아래 Cell 확장 참고
        // AddProcessor(CreateBaseProcessorAction(
        //     SignalType.ArtworkEvaluation,
        //     data.Artwork,         // Sprite (UnityEngine.Object)
        //     CalcOp.Set));


        RegisterProcessor(SignalType.OnDraw, data.onDrawActions);
        RegisterProcessor(SignalType.OnSelect, data.onSelectActions);
        RegisterProcessor(SignalType.OnUnSelect, data.onUnSelectActions);
        RegisterProcessor(SignalType.OnRequirement, data.onRequirementActions);
        RegisterProcessor(SignalType.OnUse, data.onUseActions);
        RegisterProcessor(SignalType.OnEffect, data.onEffectActions);
        RegisterProcessor(SignalType.OnPlayed, data.onPlayedActions);
        RegisterProcessor(SignalType.OnDiscard, data.onDiscardActions);
        RegisterProcessor(SignalType.OnExhaust, data.onExhaustActions);
        RegisterProcessor(SignalType.OnDestroy, data.onDestroyActions);
    }

    public override void Fire(SignalBus bus)
    {
        // 버스에 탑승시키고 처리 시작
        bus.AddPassengers(BuildBubblesForSignal(bus));
        bus.SetSourceInfo(this);
        ReactionStackManager.Instance.PushBus(bus);
    }
}