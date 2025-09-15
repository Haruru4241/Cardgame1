using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "CardGame/Actions/GainResourceOnDraw")]
public class GainResourceOnDraw : BaseAction
{
    public int amount = 1;

    public override void Execute(SignalBus Bus)
    {
        PlayerStats.Instance.AddMoney(amount);
    }
}
