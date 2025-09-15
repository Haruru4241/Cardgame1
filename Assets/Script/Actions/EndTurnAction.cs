using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "CardGame/Actions/EndTurnAction")]
public class EndTurnAction : BaseAction
{
    // 옵션: 핸드/사용/버려질 PileType을 필드로 노출하면 에디터에서 수정 가능
    public PileType handPile    = PileType.Hand;
    public PileType usedPile    = PileType.Used;
    public PileType discardPile = PileType.Discard;

    public override void Execute(SignalBus Bus)
    {
        // 안전 체크
        var used    = DeckManager.Instance.GetPile(usedPile);
        var hand    = DeckManager.Instance.GetPile(handPile);
        var discard = DeckManager.Instance.GetPile(discardPile);

        if (hand == null || discard == null)
        {
            Debug.LogWarning("[EndTurnAction] 필수 Pile 누락. 처리 중단.");
            return;
        }

        // used -> discard (있다면)
        if (used != null && used.Cards.Count > 0)
        {
            DeckManager.Instance.MigratePileCards(new List<BaseInstance>(used.Cards), discard);
        }

        // hand -> discard
        if (hand.Cards.Count > 0)
        {
            DeckManager.Instance.MigratePileCards(new List<BaseInstance>(hand.Cards), discard);
        }

        // UI 갱신 (DeckManager에서 제공하는 유틸)
        var dm = DeckManager.Instance;
        dm.ReloadCustomUI(dm.GetPile(PileType.Hand).Cards);
        //DeckManager.Instance.ReloadHandUI();

        // (옵션) 로그 남기기
        GameManager.Instance._logs += "[EndTurnAction] 핸드/사용 카드 → 버림 처리\n";
    }
}
