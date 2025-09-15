using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/ManaAction")]
public class ManaAction : BaseAction
{
    public enum ManaOp { Add, Subtract, Set }
    public ManaOp operation = ManaOp.Add;
    public int amount = 1;

    public override void Execute(SignalBus Bus)
    {
        Apply();
    }

    private void Apply()
    {
        switch (operation)
        {
            case ManaOp.Add: PlayerStats.Instance.AddMana(amount); break;
            case ManaOp.Subtract: PlayerStats.Instance.SpendMana(amount); break;
            case ManaOp.Set: PlayerStats.Instance.SetMana(amount); break;
        }
    }
}
