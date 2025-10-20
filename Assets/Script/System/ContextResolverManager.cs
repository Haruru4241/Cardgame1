// Assets/Script/Manager/ContextResolverManager.cs (이 코드로 교체하세요)

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ContextResolverManager : MonoBehaviour
{
    public static ContextResolverManager Instance { get; private set; }

    [SerializeField]
    private ContextRuleSet _ruleSet; // 인스펙터에서 규칙 세트 SO를 연결

    void Awake() { Instance = this; }

    /// <summary>
    /// ★★★ 최종 통합 메서드 (단순화됨) ★★★
    /// 주어진 인스턴스의 '위치'를 판단하여, 관련된 모든 인스턴스 목록을 반환합니다.
    /// </summary>
    /// <param name="sourceInstance">상황 판단의 주체가 되는 인스턴스</param>
    /// <returns>수집된 관련 인스턴스 목록</returns>
    public List<BaseInstance> GetRelevantInstances(BaseInstance sourceInstance)
    {
        if (sourceInstance == null) return new List<BaseInstance>();

        // 1. 현재 '위치'만 판별합니다.
        Pile currentPile = sourceInstance.CurrentZone as Pile;
        PileType currentLocation = currentPile != null ? currentPile.Type : PileType.None;

        // 2. 판별된 위치를 바탕으로 어떤 대상을 수집할지 '규칙'을 가져옵니다.
        CollectorType collectorsToUse = GetCollectorsForLocation(currentLocation); // 내부 메서드 이름 변경

        // 3. 결정된 규칙에 따라 실제 인스턴스들을 '수집'하여 반환합니다.
        return CollectInstances(collectorsToUse, sourceInstance);
    }


    // --- 내부 private 헬퍼 메서드들 ---

    /// <summary>
    /// [내부 메서드 1] 규칙 세트(SO)를 읽어 현재 '위치'에 가장 적합한 '수집 대상' 마스크를 찾습니다.
    /// </summary>
    private CollectorType GetCollectorsForLocation(PileType currentLocation) // 메서드 이름 변경
    {
        ContextRuleEntry bestMatch = null;
        foreach (var rule in _ruleSet.Rules)
        {
            // ★★★ 이제 위치(ZoneMask) 조건만 검사합니다 ★★★
            if (rule.RequiredZoneMask != PileType.None && (currentLocation & rule.RequiredZoneMask) == 0) continue;
            
            // 우선순위 비교 (여전히 유효)
            if (bestMatch == null || rule.Priority > bestMatch.Priority)
            {
                bestMatch = rule;
            }
        }
        return bestMatch != null ? bestMatch.CollectorMask : _ruleSet.DefaultCollectors;
    }

    /// <summary>
    /// [내부 메서드 2] CollectorType 마스크를 기반으로 실제 인스턴스 목록을 수집합니다.
    /// (이 메서드는 이전과 동일하게 작동합니다)
    /// </summary>
    private List<BaseInstance> CollectInstances(CollectorType collectors, BaseInstance source)
    {
        var collectedInstances = new List<BaseInstance>();

        if ((collectors & CollectorType.Global) != 0 && DeckManager.Instance.Rule != null)
            collectedInstances.Add(DeckManager.Instance.Rule);
        if ((collectors & CollectorType.Owner) != 0 && source.Owner != null)
            collectedInstances.Add(source.Owner);
        if ((collectors & CollectorType.Source) != 0)
            collectedInstances.Add(source);
        if ((collectors & CollectorType.Target) != 0 && InputManager.Instance.HoveredTarget != null)
            collectedInstances.Add(InputManager.Instance.HoveredTarget);
        // Zone 수집 로직은 필요시 추가...

        return collectedInstances.Distinct().ToList();
    }

    // DetermineCurrentConditionFor 메서드는 완전히 제거됩니다.
}