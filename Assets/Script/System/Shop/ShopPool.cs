using UnityEngine;
using System.Collections.Generic;
using System.Linq; // [!!!] LINQ (Except, ToList)를 사용하기 위해 필수

[CreateAssetMenu(fileName = "New Shop Pool", menuName = "Shop/Shop Pool")]
public class ShopPool : ScriptableObject
{
    [Tooltip("이 풀에서 등장할 수 있는 BaseData 목록")]
    public List<BaseData> availableItems;

    /// <summary>
    /// 'excludeList'를 제외하고, 'rarityMask'와 일치하는 아이템만 뽑습니다.
    /// </summary>
    /// <param name="excludeList">이미 상점에 뽑혀서 제외할 아이템</param>
    /// <param name="rarityMask">슬롯이 허용하는 등급 마스크 (예: Common | Rare)</param>
    public BaseData GetRandomItem(List<BaseData> excludeList, Rarity rarityMask)
    {
        if (availableItems == null || availableItems.Count == 0) return null;

        // 1. [!] 먼저 '전체 풀'에서 'rarityMask'와 일치하는 아이템만 필터링
        var filteredByRarity = availableItems.Where(item =>
            // 비트 AND(&) 연산:
            // (Common|Rare (3) & Rare (2)) == 2  -> true
            // (Common|Rare (3) & Epic (4)) == 0  -> false
            (rarityMask & item.rarity) != 0
        ).ToList();

        List<BaseData> poolToUse;

        // 2. '제외 목록(excludeList)'이 있는지 확인
        if (excludeList != null && excludeList.Count > 0)
        {
            // 3. '등급 필터링된 풀'에서 '제외 목록'을 뺀 '최종 풀' 계산
            poolToUse = filteredByRarity.Except(excludeList).ToList();
        }
        else
        {
            poolToUse = filteredByRarity;
        }

        if (poolToUse.Count == 0)
        {
            return null; // 뽑을 수 있는 아이템이 없음
        }

        int randomIndex = Random.Range(0, poolToUse.Count);
        return poolToUse[randomIndex];
    }
}