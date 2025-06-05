using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/DrawCardsAction")]
public class DrawCardsAction : CardAction
{
    [Header("드로우 수")]
    public int drawCount = 3;
    public override void Execute(CardInstance card, Processor processor)
    {
        var func = GetFunction(null);
        func?.Invoke(null);  // input이 필요 없으면 null
    }

    public override Func<object, object> GetFunction(Processor processor)
    {
        return _ =>
        {
            //DeckManager.Instance.DrawCards(drawCount);
            GameManager.Instance._logs += " 드로우 시작 ";
            int drawn = 0;
            for (int i = 0; i < drawCount; i++)
            {
                if (DeckManager.Instance.DeckPile.Cards.Count == 0)
                {
                    DeckManager.Instance.MigratePileCards(DeckManager.Instance.DiscardPile.FindAll(c => true), DeckManager.Instance.DeckPile, true);
                }        // **변경**: 덱 비었으면 즉시 재활용

                if (DeckManager.Instance.DeckPile.Cards.Count == 0)
                    break;

                var ci = DeckManager.Instance.DeckPile.Cards[0];

                if (ci.BaseCard != null)
                {
                    ci.BaseCard.cardInstance.Fire(SignalType.OnDraw);
                }

                drawn++;
            }
            GameManager.Instance._logs += " \n\n ";
            DeckManager.Instance.ReloadHandUI();
            return null;
        };
    }
}
