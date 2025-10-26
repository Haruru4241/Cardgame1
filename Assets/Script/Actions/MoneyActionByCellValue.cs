using UnityEngine;
using System;

[CreateAssetMenu(menuName = "CardGame/Actions/MoneyAction By CellValue")]
public class MoneyActionByCellValue : BaseAction
{
    public enum MoneyOp { Add, Subtract, Set }
    public MoneyOp operation = MoneyOp.Add;

    public override void Execute(SignalBus bus)
    {
        int amount = CalculationManager.Instance.Evaluate<int>(bus, CalcType.Money);
        // 1. Cell에서 지정된 키로 값을 가져옵니다.
        // if (bus == null || bus.CalcKind != CellKind.Int) return;

        // // 1) 셀에서 드로우 개수 해석
        // int amount = Mathf.Max(0, (int)bus.CalcRaw);
        //GameManager.Instance._logs += $"Money{operation}{amount} ";
        EventManager.Instance.LogEvent(LogType.Global, $"Money{operation}{amount}", bus.Signal, null, null, bus);

        // 2. 가져온 값으로 돈을 조작합니다.
        Apply(amount);
    }

    private void Apply(int amount)
    {
        switch (operation)
        {
            case MoneyOp.Add: PlayerStats.Instance.AddMoney(amount); break;
            case MoneyOp.Subtract: PlayerStats.Instance.SpendMoney(amount); break;
            case MoneyOp.Set: PlayerStats.Instance.SetMoney(amount); break;
        }
        UIManager.Instance.SetScore(PlayerStats.Instance.money);
    }
}