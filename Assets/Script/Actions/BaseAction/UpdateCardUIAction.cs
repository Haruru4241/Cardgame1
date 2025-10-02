using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Actions/Update Card UI")]
public class UpdateCardUIAction : BaseAction
{
    public override void Execute(SignalBus bus)
    {
        bus.GetSourceCard()?.controller?.UpdateUI();
    }
}