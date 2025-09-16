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
        var candidates = GetCandidates();

        if (candidates.Count == 0)
        {
            Debug.LogWarning("후보가 없습니다.");
            return;
        }
        var selectState = GameManager.Instance.SelectState as SelectState;

        selectState.StartSelection(
            candidates,
            Mathf.Min(requiredCount, candidates.Count),
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

        return result;
    }


    private void OnSelectionFinished(List<BaseInstance> list, SignalBus bus)
    {
        foreach (var inst in list)
        {
            // 🔹 선택된 인스턴스에서 OnEffect 발동
            inst.Fire(new SignalBus(SignalType.OnEffect, bus));
        }
    }
}
