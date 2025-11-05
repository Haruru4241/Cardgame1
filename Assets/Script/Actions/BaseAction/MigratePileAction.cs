using UnityEngine;
using System.Collections.Generic;

// (가정) PileType enum이 Deck, Discard 등을 정의
// (가정) ListExtensions.cs에 'Shuffle()' 확장 메소드가 존재

[CreateAssetMenu(fileName = "MigratePileAction", menuName = "CardGame/Actions/Migrate Pile")]
public class MigratePileAction : BaseAction
{
    [Header("파일(Pile) 이동 설정")]
    [Tooltip("카드를 가져올 원본 파일입니다. (예: Discard)")]
    public ZoneType Source;

    [Tooltip("카드를 보낼 목적지 파일입니다. (예: Deck)")]
    public ZoneType Destination;

    [Header("옵션")]
    [Tooltip("true일 경우, 카드를 Destination으로 옮긴 후 Destination 파일을 섞습니다.")]
    public bool ShuffleOnMigrate;

    // (가정) 이 액션은 ValueAction 등과 연계되지 않으므로 
    //       단순히 Execute만 구현합니다.
    public override void Execute(SignalBus bus)
    {
        // (가정) DeckManager 싱글톤에 접근
        var manager = DeckManager.Instance;

        var fromPile = manager.GetPile(Source);
        var toPile = manager.GetPile(Destination);

        // 유효성 검사
        if (fromPile == null || toPile == null)
        {
            Debug.LogError($"[MigratePileAction] Source 또는 Destination 파일이 유효하지 않습니다.");
            return;
        }

        if (toPile.Cards.Count == 0)
        {
            EventManager.Instance.LogEvent(LogType.Shuffle,
    $"{Source}에서 {Destination}로 {toPile.Cards.Count}장 이동 (셔플: {ShuffleOnMigrate})",
    bus.Signal, null, null, bus);
            manager.MigratePileCards(fromPile.FindAll(_ => true), toPile, true);// 4. 셔플 옵션이 켜져 있으면 목적지 파일을 섞음
            if (ShuffleOnMigrate)
            {
                toPile.Shuffle();
            }
        }
    }
}