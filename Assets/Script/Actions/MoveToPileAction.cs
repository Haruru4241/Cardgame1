using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/MoveToPile")]
public class MoveToPileAction : CardAction
{
    public enum TargetPileType { Used, Discard, Exhaust, Deck, Hand, Destroy}
    public TargetPileType targetPile;

    public override void Execute(CardInstance card)
    {
        Apply(card);
    }

    public override Func<object, object> GetFunction(Processor processor)
    {
        return _ =>
        {
            Apply(processor.Owner);
            return null;
        };
    }

    private void Apply(CardInstance card)
    {
        DeckManager dm = DeckManager.Instance;

        // 현재 파일에서 제거
        card.CurrentPile?.Remove(card);

        // 목적지 파일로 이동
        Pile toPile = null;
        switch (targetPile)
        {
            case TargetPileType.Used: toPile = dm.UsedPile; break;
            case TargetPileType.Discard: toPile = dm.DiscardPile; break;
            case TargetPileType.Exhaust: toPile = dm.ExhaustPile; break;
            case TargetPileType.Deck: toPile = dm.DeckPile; break;
            case TargetPileType.Hand: toPile = dm.HandPile; break;
            case TargetPileType.Destroy: toPile = dm.DestroyPile; break;
        }

        toPile?.Add(card);

        // UI 갱신
        dm.ReloadHandUI();
    }
}
