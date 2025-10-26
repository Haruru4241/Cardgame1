using UnityEngine;

/// <summary>
/// 게임 세계에 존재하는 적의 논리적 인스턴스입니다.
/// 적의 현재 상태와 행동 로직을 관리합니다.
/// </summary>
public class EnemyInstance : BaseInstance
{
    /// <summary>
    /// EnemyData를 기반으로 새로운 EnemyInstance를 생성합니다.
    /// </summary>
    public EnemyInstance(EnemyData data)
    {
        // 부모의 BaseData와 자식의 EnemyData를 모두 초기화합니다.
        _data = data;
        //GameManager.Instance._logs += $"인스턴스 셋업 - ";
        EventManager.Instance.LogEvent(LogType.Debug, $"인스턴스 셋업", SignalType.Debug, null, null, null);
        // 이름
        AddProcessor(CreateBaseProcessorAction(
            SignalType.NameEvaluation,
            data.Name,            // string
            CalcType.Name,
            CalcOp.Set));

        // 설명
        AddProcessor(CreateBaseProcessorAction(
            SignalType.DescriptionEvaluation,
            data.Description,     // string
            CalcType.Description,
            CalcOp.Set));

        // HP
        AddProcessor(CreateBaseProcessorAction(
            SignalType.HPEvaluation,
            data.Health,        // int
            CalcType.Health,
            CalcOp.Set));

        // 피해량
        AddProcessor(CreateBaseProcessorAction(
            SignalType.DealDamageEvaluation,
            data.Damage,        // int
            CalcType.DealDamage,
            CalcOp.Set));

        // 구매 비용
        AddProcessor(CreateBaseProcessorAction(
            SignalType.BuyCostEvaluation,
            data.BuyCost,            // int
            CalcType.Cost,
            CalcOp.Set));

        // 데이터에 정의된 액션들을 프로세서로 등록합니다.
        foreach (var entry in data.actionEntries)
        {
            RegisterProcessor(entry.signal, entry.actions);
        }
    }
    public override void Fire(SignalBus bus)
    {
        var Bubbles = BuildBubblesForSignal(bus);
        // 버스에 탑승시키고 처리 시작
        if (Bubbles.Count == 0) return;
        bus.AddPassengers(Bubbles);
        bus.SetSourceInfo(this);
        //GameManager.Instance._logs += $"fire {bus.Signal}{bus._bubbles.Count} ";
        EventManager.Instance.LogEvent(LogType.CardPlayed, $"fire {bus.Signal}{bus._bubbles.Count}", bus.Signal, null, null, bus);
        ReactionStackManager.Instance.PushBus(bus);
    }
    /// <summary>
    /// 최종 계산된 피해량을 받아 체력을 감소시킵니다.
    /// </summary>
    // public void TakeDamage(int damage)
    // {
    //     CurrentHealth -= damage;
    //     Debug.Log($"    > 최종 피해 {damage} 적용! 남은 체력: {CurrentHealth}");
    //     Controller?.UpdateUI();
    //     if (CurrentHealth <= 0) Die();
    // }

    // private void Die()
    // {
    //     Fire(SignalType.OnDestroy);
    // }
}