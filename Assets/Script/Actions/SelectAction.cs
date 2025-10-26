using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Actions/SelectAction")]
public class SelectAction : BaseAction
{
    [Header("선택 후보 데이터")]
    public List<BaseData> candidateData;   // 🔹 여러 후보 등록 가능
    public int requiredCount = 1;

    public override void Execute(SignalBus bus)
    {
        var selectState = GameManager.Instance.SelectState as SelectState;

        selectState.StartSelection(
            () => GetCandidates(),
            requiredCount,
            list => OnSelectionFinished(list, bus),
            bus
        );
    }

    private List<BaseInstance> GetCandidates()
    {
        var result = new List<BaseInstance>();

        foreach (var d in candidateData)
        {
            // 임시 인스턴스 생성 (UI에 보여줄 용도)
            var ci = DeckManager.Instance.CreateInstanceFromData((CardData)d, DeckManager.Instance.dumpArea, true);

            // 후보 목록에 추가
            result.Add(ci);
        }
        //GameManager.Instance._logs += $"\n 선택 후보자 {result.Count} ";
        EventManager.Instance.LogEvent(LogType.Global, $"\n 선택 후보자 {result.Count}", SignalType.Action);

        return result;
    }


    private void OnSelectionFinished(List<BaseInstance> list, SignalBus bus)
    {
        var busesToPush = new List<SignalBus>();

        foreach (var ci in list)
        {
            busesToPush.Add(ci.PrepareBus(new SignalBus(SignalType.OnEffect, bus)));
            
        }

        // 3. 모든 준비가 끝난 후, 한 번에 출발시킵니다.
        if (busesToPush.Count > 0)
            ReactionStackManager.Instance.PushBuses(busesToPush); // PushSequence 사용 권장
    }
}
