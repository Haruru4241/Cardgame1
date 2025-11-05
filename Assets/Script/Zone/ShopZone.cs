using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// [!!!] '손(Hand)' 영역입니다.
/// 'Zone'을 상속받으며, '구매' 및 '손패 정리' 로직을 담당합니다.
/// </summary>
public class ShopZone : Zone 
{
    void Start()
    {
        // [!] 이 존은 'Hand' 타입
        this.zoneType = ZoneType.Hand; 
    }
    
    /// <summary>
    /// [!] (Zone 오버라이드) 
    /// '손' 영역에서는 '드래그 시작'을 허용합니다. (카드 사용을 위해)
    /// </summary>
    public bool CanDragFrom(BaseInstance card)
    {
        // (가정) 턴/비용 등에 따라 드래그 가능 여부 체크
        return true; 
    }
    
    /// <summary>
    /// [!] (Zone 오버라이드)
    /// 카드가 '손' 영역(구매 영역)에 '드롭'되었을 때의 로직입니다.
    /// </protected>
    protected void HandleDrop(BaseInstance card)
    {
        // [!!!] 요청하신 '소속'에 따른 로직 분기
        
        if (card.CurrentZone.zoneType == ZoneType.Shop)
        {
            // [1] "상점"에서 온 카드 -> "구매" 로직 실행
            ShopManager.Instance.RequestBuy(card);
        }
        else
        {
            // [3] 그 외 (덱, 플레이 영역 등) -> 복귀 (허용 안 됨)
            card.controller.ReturnToOriginalParent();
        }
    }
}