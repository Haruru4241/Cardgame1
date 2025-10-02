using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/Highlight")]
public class HighlightAction : BaseAction
{
    public bool highlight = true;

    public override void Execute(SignalBus Bus)
    {
        Bus.GetSourceCard().controller.transform.localScale = highlight ? Vector3.one * 1.1f : Vector3.one;
    }
}
