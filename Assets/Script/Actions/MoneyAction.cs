using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/MoneyAction")]
public class MoneyAction : BaseAction
{
    public enum MoneyOp { Add, Subtract, Set }
    public MoneyOp operation = MoneyOp.Add;
    public int amount = 1;

    public override void Execute(SignalBus Bus)
    {
        Apply();
    }

    private void Apply()
    {
        switch (operation)
        {
            case MoneyOp.Add: PlayerStats.Instance.AddGold(amount); break;
            case MoneyOp.Subtract: PlayerStats.Instance.SpendGold(amount); break;
            case MoneyOp.Set: PlayerStats.Instance.SetGold(amount); break;
        }
        UIManager.Instance.SetScore(PlayerStats.Instance.money);
    }
}
