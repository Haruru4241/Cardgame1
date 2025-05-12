using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/DrawCardsAction")]
public class DrawCardsAction : CardAction
{
    [Header("드로우 수")]
    public int drawCount = 3;
    public override void Execute(CardInstance card)
    {
        var func = GetFunction(null);
        func?.Invoke(null);  // input이 필요 없으면 null
    }

    public override Func<object, object> GetFunction(Processor processor)
    {
        return _ =>
        {
            DeckManager.Instance.DrawCards(drawCount);
            return null;
        };
    }
}
