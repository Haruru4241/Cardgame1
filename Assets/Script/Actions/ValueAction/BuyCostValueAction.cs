using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Actions/BuyCostValueAction")]
public class BuyCostValueAction : ValueAction
{
    public override object GetValue(SignalBus bus)
    {
        var src = bus.GetSourceCard();
        if (src != null && src._data is CardData cd)
            return cd.BuyCost;
        return 0;
    }
}