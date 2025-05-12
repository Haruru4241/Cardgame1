using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/TargetSelector")]
public class TargetSelector : ScriptableObject
{
    [Header("기본 옵션")]
    [Tooltip("자기 자신 제외")]
    public bool excludeSelf = true;

    [Tooltip("같은 CardData만 허용")]
    public bool matchSameData = false;

    [Tooltip("같은 카드 이름만 허용")]
    public bool matchSameName = false;

    [Tooltip("같은 팩션만 허용")]
    public bool matchSameFaction = false;

    [Header("특정 카드 데이터 필터")]
    [Tooltip("이 리스트에 포함된 CardData만 허용 (비어 있으면 무시)")]
    public List<CardData> allowCardDatas = new List<CardData>();

    [Header("코스트 범위 필터")]
    [Tooltip("코스트 필터 사용")]
    public bool useCostRange = false;

    [Tooltip("최소 코스트 (useCostRange가 true일 때)")]
    public int minCost = 0;

    [Tooltip("최대 코스트 (useCostRange가 true일 때)")]
    public int maxCost = 0;

    [Header("커스텀 개별 대상 리스트")]
    [Tooltip("여기에 직접 지정한 인스턴스만 추가 (비어 있으면 무시)")]
    public List<CardInstance> manualTargets = new List<CardInstance>();

    /// <summary>
    /// 설정에 맞춰 대상을 추출합니다.
    /// </summary>
    public List<CardInstance> GetTargets(CardInstance origin)
    {
        // 1) 초기 후보: AllCardInstances + manualTargets
        var all = DeckManager.Instance.AllCardInstances;
        IEnumerable<CardInstance> candidates = all;

        // 2) 수동 지정 리스트가 있으면, 그것만
        if (manualTargets != null && manualTargets.Count > 0)
        {
            candidates = manualTargets;
        }

        // 3) excludeSelf
        if (excludeSelf)
            candidates = candidates.Where(c => c != origin);

        // 4) matchSameData
        if (matchSameData)
            candidates = candidates.Where(c => c.CardData == origin.CardData);

        // 5) matchSameName
        if (matchSameName)
            candidates = candidates.Where(c =>
                c.CardData.cardName.Equals(origin.CardData.cardName));

        // 6) matchSameFaction
        if (matchSameFaction)
            candidates = candidates.Where(c =>
                c.CardData.faction == origin.CardData.faction);

        // 7) allowCardDatas
        if (allowCardDatas != null && allowCardDatas.Count > 0)
            candidates = candidates.Where(c =>
                allowCardDatas.Contains(c.CardData));

        // 8) cost range
        if (useCostRange)
            candidates = candidates.Where(c =>
                c.CardData.cost >= minCost && c.CardData.cost <= maxCost);

        // 9) 최종 반환
        return candidates.Distinct().ToList();
    }
}
