using System.Collections.Generic;
using System;                  // Action 델리게이트를 위해
using System.Linq;             // ToList() 확장 메서드를 위해
using UnityEngine;
using Unity.VisualScripting;
public class CardInstance : BaseInstance
{
    public CardInstance(CardData data)
    {
        _data = data;

        CurrentZone = null;
        SetupBaseProcessors(data);
    }
    // 3) 초기값 프로세서 등록 예시
    public void SetupBaseProcessors(CardData data)
    {
        //GameManager.Instance._logs += $"인스턴스 셋업 - ";
        EventManager.Instance.LogEvent(LogType.Debug, $"인스턴스 셋업", SignalType.Debug, null, null, null);
        // 이름
        AddProcessor(CreateBaseProcessorAction(
            SignalType.NameEvaluation,
            data.Name,            // string
            CalcType.Name,
            CalcOp.Set));

        // 설명
        AddProcessor(CreateBaseProcessorAction(
            SignalType.DescriptionEvaluation,
            data.Description,     // string
            CalcType.Description,
            CalcOp.Set));

        // 마나 코스트
        AddProcessor(CreateBaseProcessorAction(
            SignalType.ManaCostEvaluation,
            data.manaCost,        // int
            CalcType.ManaCost,
            CalcOp.Set));

        // 구매 비용
        AddProcessor(CreateBaseProcessorAction(
            SignalType.BuyCostEvaluation,
            data.BuyCost,            // int
            CalcType.Cost,
            CalcOp.Set));

        // 아트워크 (Sprite)  ← 아래 Cell 확장 참고
        // AddProcessor(CreateBaseProcessorAction(
        //     SignalType.ArtworkEvaluation,
        //     data.Artwork,         // Sprite (UnityEngine.Object)
        //     CalcOp.Set));

        foreach (var entry in data.actionEntries)
        {
            // 엔트리에 액션이 하나라도 있을 경우에만 등록
            if (entry.actions != null && entry.actions.Count > 0)
            {
                RegisterProcessor(entry.signal, entry.actions);
            }
        }
    }

    public override void Fire(SignalBus bus)
    {
        List<BaseInstance> relevantInstances = ContextResolverManager.Instance.GetRelevantInstances(this);
        foreach(var a in relevantInstances)
        {
            a.PrepareBus(bus);
        }
        bus.SetSourceInfo(this);
        EventManager.Instance.LogEvent(LogType.CardPlayed, $"fire{bus.Signal}{bus._bubbles.Count}", bus.Signal, this, null, bus);
        //GameManager.Instance._logs += $"fire {bus.Signal}{bus._bubbles.Count} ";
        ReactionStackManager.Instance.PushBus(bus);
    }
}