using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 전투 필드에 존재하는 개체(유닛, 적 등)들을 관리하는 Zone의 추상 클래스입니다.
/// </summary>
public abstract class EntityZone : Zone
{
    // 이 Zone에 속한 모든 인스턴스의 목록
    protected List<BaseInstance> instancesInZone = new List<BaseInstance>();

    /// <summary>
    /// 이 Zone에 새로운 인스턴스를 추가합니다.
    /// </summary>
    public override void Add(BaseInstance instance)
    {
        if (!instancesInZone.Contains(instance))
        {
            instancesInZone.Add(instance);
            instance.CurrentZone = this; // 인스턴스에게 현재 Zone이 어디인지 알려줍니다.
            // TODO: Zone에 들어올 때의 시각적 배치 로직 (예: 카드 정렬)
        }
    }

    /// <summary>
    /// 이 Zone에서 인스턴스를 제거합니다.
    /// </summary>
    public override bool Remove(BaseInstance instance)
    {
        if (instancesInZone.Contains(instance))
        {
            instancesInZone.Remove(instance);
            return true;
            // TODO: Zone에서 나갈 때의 처리 로직
        }
        return false;
    }
}