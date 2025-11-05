using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;                  // Action 델리게이트를 위해

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
    public ZoneType zone = ZoneType.None;
    /// <summary>
    /// 카드를 가져올 파일(더미)의 위치를 지정합니다.
    /// </summary>
    public enum PilePosition
    {
        Top,    // 덱 맨 위 (Top)
        Bottom, // 덱 맨 아래 (Bottom)
        Random  // 무작위
    }
    [Header("2. 위치 설정")]
    [Tooltip("파일의 어느 부분에서 가져올지 지정합니다. 0은 '전체'를 의미")]
    public PilePosition Position;

    [Header("3. 수량 설정")]
    [Tooltip("몇 장을 대상으로 할지 결정합니다. (0 = 파일 전체)")]
    public int Amount;

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

        if (useBusTargetAsPrimary && bus != null)
        {
            candidates = CalculationManager.Instance.Evaluate<List<BaseInstance>>(bus, CalcType.TargetList) ?? new List<BaseInstance>();
        }
        if (enemiesOnly)
        {
            candidates = BattleManager.Instance.enemyInstances;
        }
        if (zone != ZoneType.None)
        {
            // 1. 'candidates'를 'zone' 기준으로 필터링하고,
            //    즉시 '.ToList()'로 변환하여 'filteredList'에 저장합니다.
            var filteredList = candidates.Where(c =>
                c.CurrentZone is Pile currentPile && (zone & currentPile.Type) != 0
            ).ToList(); // ToList()로 즉시 리스트 생성

            // [수정] 이제 'sourceList'가 아닌 'filteredList'를 기준으로 작업합니다.
            int amount = Amount;

            // 0은 '전체'를 의미
            if (amount == 0)
            {
                amount = filteredList.Count; // filteredList.Count 사용
            }

            // 리스트의 총 개수보다 많은 수를 요청하지 않도록 보정
            int count = Mathf.Min(amount, filteredList.Count); // filteredList.Count 사용

            // 2. 위치(Position)에 따라 'filteredList'의 다른 부분을 가져옴
            switch (Position)
            {
                // [덱 탑] (Top)
                case PilePosition.Top:
                    // filteredList의 맨 앞에서 'count'개수만큼 가져옴
                    return filteredList.Take(count).ToList();

                // [덱 밑] (Bottom)
                case PilePosition.Bottom:
                    // filteredList의 (전체 개수 - count)만큼 건너뛰고, 나머지를 가져옴
                    return filteredList.Skip(filteredList.Count - count).ToList();

                // [무작위] (Random)
                case PilePosition.Random:
                    // filteredList를 무작위로 섞은 뒤 'count'개수만큼 가져옴
                    return filteredList.OrderBy(c => System.Guid.NewGuid()).Take(count).ToList();

                default:
                    // 유효하지 않은 Position일 경우, 빈 리스트 반환 (기존 로직 유지)
                    return new List<BaseInstance>();
            }
        }
        if (origin != null)
        {
            // 3) excludeSelf
            if (excludeSelf)
                candidates = candidates.Where(c => c != origin);
            else if (selectSelfOnly)
                // candidates = candidates.Where(c => c == origin);
                return new List<BaseInstance> { origin };

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
