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
    public List<BaseInstance> manualTargets = new List<BaseInstance>();

    /// <summary>
    /// 설정에 맞춰 대상을 추출합니다.
    /// </summary>
    public List<BaseInstance> GetTargets(BaseInstance origin)
    {

        // 1) 초기 후보: AllCardInstances + manualTargets
        var all = DeckManager.Instance.AllInstances;
        IEnumerable<BaseInstance> candidates = all;

        candidates = candidates.Where(c => !(c is RuleInstance));

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
            candidates = candidates.Where(c => c.BaseData == origin.BaseData);

        // 5) matchSameName
        if (matchSameName)
            candidates = candidates.Where(c =>
    {
        DebugCheck(origin, c); // ★ 의심 구간 검사 로그
        return string.Equals(c?.BaseData?.Name, origin?.BaseData?.Name);
    });

        // 6) matchSameFaction
        if (matchSameFaction)
            candidates = candidates.Where(c =>
                c.BaseData.faction == origin.BaseData.faction);

        // 7) allowCardDatas
        if (allowCardDatas != null && allowCardDatas.Count > 0)
            candidates = candidates.Where(c =>
                allowCardDatas.Contains(c.BaseData));

        // 8) cost range
        if (useCostRange)
            candidates = candidates.Where(c =>
                c.BaseData.Cost >= minCost && c.BaseData.Cost <= maxCost);

        // 9) 최종 반환
        return candidates.Distinct().ToList();
    }
    private void DebugCheck(BaseInstance origin, BaseInstance c)
    {
        if (c == null)
        {
            Debug.LogWarning("[TargetSelector] candidate 자체가 null 입니다.");
            return;
        }

        if (c.BaseData == null)
        {
            Debug.LogWarning($"[TargetSelector] {c.BaseData} 의 BaseData가 null 입니다.");
            Debug.LogWarning($"{c.BaseCard.nameText.text}{c.BaseCard.CardData.Name}");
            return;
        }

        if (origin == null)
        {
            Debug.LogWarning("[TargetSelector] origin 자체가 null 입니다.");
            return;
        }

        if (origin.BaseData == null)
        {
            Debug.LogWarning($"[TargetSelector] origin({origin.BaseData}) 의 BaseData가 null 입니다.");
            return;
        }

        if (string.IsNullOrEmpty(c.BaseData.Name))
        {
            Debug.LogWarning($"[TargetSelector] {c.BaseData.Name} 의 BaseData.Name 이 null 혹은 빈 문자열 입니다.");
        }
    }

}
