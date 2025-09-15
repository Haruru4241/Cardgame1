using System;
using System.Collections.Generic;
using System.Linq;    // LINQ 확장을 쓰기 위함
using UnityEngine;

public static class CardSearchUtility
{
    // 모든 카드 인스턴스 shortcut
    private static List<BaseInstance> All => DeckManager.Instance.AllInstances;

    /// <summary>
    /// self와 동일한 CardData를 갖고, self 인스턴스가 아닌 카드만 반환
    /// </summary>
    public static List<BaseInstance> FindSameDataExceptSelf(BaseInstance self)
    {
        return All
            .Where(c => c != self && c.BaseData == self.BaseData)
            .ToList();
    }

    /// <summary>
    /// self와 이름(cardName)이 같고, self 인스턴스가 아닌 카드만 반환
    /// </summary>
    public static List<BaseInstance> FindSameNameExceptSelf(BaseInstance self)
    {
        return All
            .Where(c => c != self && 
                        c.BaseData.Name.Equals(self.BaseData.Name, StringComparison.Ordinal))
            .ToList();
    }

    /// <summary>
    /// 다양한 조건을 플래그로 지정해서 검색할 수 있는 범용 검색기
    /// </summary>
    public static List<BaseInstance> FindByCriteriaExceptSelf(
        BaseInstance self,
        bool matchData = false,
        bool matchName = false,
        bool matchFaction = false,
        int? maxCost = null
    )
    {
        IEnumerable<BaseInstance> query = All.Where(c => c != self);

        if (matchData)
            query = query.Where(c => c.BaseData == self.BaseData);

        if (matchName)
            query = query.Where(c => c.BaseData.Name.Equals(
                self.BaseData.Name, StringComparison.Ordinal));

        if (matchFaction)
            query = query.Where(c => c.BaseData.faction == self.BaseData.faction);

        if (maxCost.HasValue)
            query = query.Where(c => c.BaseData.Cost <= maxCost.Value);

        return query.ToList();
    }

    /// <summary>
    /// 람다식으로 직접 조건을 넘겨주는 가장 유연한 검색기
    /// </summary>
    public static List<BaseInstance> FindExceptSelf(
        BaseInstance self,
        Func<BaseInstance, bool> predicate
    )
    {
        return All
            .Where(c => c != self && predicate(c))
            .ToList();
    }
}
