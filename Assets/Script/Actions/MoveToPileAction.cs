using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/MoveToPile")]
public class MoveToPileAction : BaseAction
{
    // ✅ 전역 PileType 재사용 (중복 enum 제거)
    public PileType targetPile;

    public override void Execute(SignalBus Bus)
    {
        Apply(Bus.GetSourceCard());
    }

    private void Apply(BaseInstance card)
    {
        if (card == null) return;

        var dm = DeckManager.Instance;
        if (dm == null) return;

        // ✅ 새 구조: 타입으로 목적지 Pile 검색
        var toPile = dm.GetPile(targetPile);
        if (toPile == null)
        {
            Debug.LogWarning($"[MoveToPile] 대상 Pile({targetPile})이 활성화/생성되지 않았습니다.");
            return;
        }

        // ✅ 같은 곳이면 작업 불필요
        if (card.CurrentZone == toPile)
        {
            return;
        }

        // 현재 파일에서 제거 (안전 가드)
        card.CurrentZone?.Remove(card);

        // 목적지 파일로 이동
        toPile.Add(card);

        // UI 갱신 (핸드 변화에만 의존해도 되지만, 일단 안전하게 호출)
        //dm.ReloadHandUI();
        dm.ReloadCustomUI(dm.GetPile(PileType.Hand).Cards);
    }
}
