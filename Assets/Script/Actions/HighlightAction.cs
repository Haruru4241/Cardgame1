using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/Highlight")]
public class HighlightAction : CardAction
{
    public bool highlight = true;

    public override void Execute(CardInstance card, Processor processor)
    {
        card.BaseCard.transform.localScale = highlight ? Vector3.one * 1.1f : Vector3.one;
    }

    public override Func<object, object> GetFunction(Processor processor)
    {
        return _ =>
        {
            processor.Owner.BaseCard.transform.localScale = highlight ? Vector3.one * 1.1f : Vector3.one;
            return null;
        };
    }
}
