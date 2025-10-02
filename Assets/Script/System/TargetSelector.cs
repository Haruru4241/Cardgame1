using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/TargetSelector")]
public class TargetSelector : ScriptableObject
{
    [Header("기본 옵션")]
    [Tooltip("자기 자신 제외")]
    public bool excludeSelf = true;
    public bool selectSelfOnly = false;
    [Tooltip("GetTargets에 전달된 busTarget을 직접 사용하려면 체크")]
    public bool useBusTargetAsPrimary = false;

    [Tooltip("같은 CardData만 허용")]
    public bool matchSameData = false;

    [Tooltip("같은 카드 이름만 허용")]
    public bool matchSameName = false;

    [Tooltip("같은 팩션만 허용")]
    public bool matchSameFaction = false;
    [Tooltip("특정 파일 타입만 허용")]
    public PileType zone = PileType.None;

    [Tooltip("체크하면 다른 모든 조건을 무시하고 오직 적 인스턴스만 찾습니다.")]
    public bool enemiesOnly = false;

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
    public List<BaseInstance> GetTargets(BaseInstance origin = null, SignalBus bus = null)
    {
        // 1) 초기 후보: AllCardInstances + manualTargets
        var all = GameManager.Instance.AllInstances;
        IEnumerable<BaseInstance> candidates = all;

        candidates = candidates.Where(c => !(c is RuleInstance));

        // 2) 수동 지정 리스트가 있으면, 그것만
        if (manualTargets != null && manualTargets.Count > 0)
        {
            candidates = manualTargets;
        }
        if (useBusTargetAsPrimary && bus.Target != null)
        {
            return new List<BaseInstance> { bus.Target };
        }
        if (enemiesOnly)
        {
            candidates = BattleManager.Instance.enemyInstances;
        }
        if (zone != PileType.None)
        {
            candidates = candidates.Where(c =>
                c.CurrentZone is Pile currentPile && (zone & currentPile.Type) != 0
            );
        }
        if (origin != null)
        {
            // 3) excludeSelf
            if (excludeSelf)
                candidates = candidates.Where(c => c != origin);
            else if (selectSelfOnly)
                candidates = candidates.Where(c => c == origin);

            // 4) matchSameData
            if (matchSameData)
                candidates = candidates.Where(c => c._data == origin._data);

            // 5) matchSameName
            if (matchSameName)
                candidates = candidates.Where(c => string.Equals(c?._data?.Name, origin._data?.Name));

            // 6) matchSameFaction
            if (matchSameFaction)
                candidates = candidates.Where(c => c._data.faction == origin._data.faction);
        }

        // 7) allowCardDatas
        if (allowCardDatas != null && allowCardDatas.Count > 0)
            candidates = candidates.Where(c =>
                allowCardDatas.Contains(c._data));

        // 8) cost range
        if (useCostRange)
            candidates = candidates.Where(c =>
                c._data.BuyCost >= minCost && c._data.BuyCost <= maxCost);

        // 9) 최종 반환
        return candidates.Distinct().ToList();
    }
}
