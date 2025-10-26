using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/MoveToPile")]
public class MoveToPileAction : BaseAction
{
    // ✅ 전역 PileType 재사용 (중복 enum 제거)
    public PileType targetPile;

    public override void Execute(SignalBus bus)
    {
        var card = bus.GetSourceCard();
        if (card == null) return;
        

        var dm = DeckManager.Instance;
        var toPile = dm.GetPile(targetPile);
        var fromPile = (Pile)card.CurrentZone;


        if (toPile == null || card.CurrentZone == toPile) return;
        
        EventManager.Instance.LogEvent(LogType.Global, $"MoveToPileAction {targetPile}", bus.Signal, null, null, bus);

        // --- 2. 데이터 이동 (원자적 실행) ---
        fromPile?.Remove(card);
        toPile.Add(card);
        
        dm.ReloadCustomUI(dm.GetPile(PileType.Hand).Cards);
        //GameManager.Instance._logs += $"MoveToPileAction {dm.GetPile(PileType.Hand).Cards.Count}-> ";
        // --- 3. 신호 방송 ---
        // 데이터 이동이 모두 끝난 후, 관련된 파일들의 변경 신호를 순서대로 방송합니다.
        if (fromPile != null)
        {
            dm.BroadcastSignalToAllPiles(fromPile.PileSignal);
        }
        dm.BroadcastSignalToAllPiles(toPile.PileSignal);
    }

    private void Apply(BaseInstance card)
    {
        if (card == null) return;
        

        var dm = DeckManager.Instance;
        var toPile = dm.GetPile(targetPile);
        var fromPile = (Pile)card.CurrentZone;
        

        if (toPile == null || card.CurrentZone == toPile) return;

        // --- 2. 데이터 이동 (원자적 실행) ---
        fromPile?.Remove(card);
        toPile.Add(card);
        
        dm.ReloadCustomUI(dm.GetPile(PileType.Hand).Cards);
        //GameManager.Instance._logs += $"3 {dm.GetPile(PileType.Hand).Cards.Count} ";
        //EventManager.Instance.LogEvent(LogType.Global, $"Money{operation}{amount}", bus.Signal, null, null, bus);
        // --- 3. 신호 방송 ---
        // 데이터 이동이 모두 끝난 후, 관련된 파일들의 변경 신호를 순서대로 방송합니다.
        if (fromPile != null)
        {
            dm.BroadcastSignalToAllPiles(fromPile.PileSignal);
        }
        dm.BroadcastSignalToAllPiles(toPile.PileSignal);
    }
}
