using UnityEngine;
using System.Linq;

/// <summary>
/// 이번 턴에 사용된 카드의 수가 지정된 수치(requiredCount) 이상인지 확인하는 조건입니다.
/// </summary>
[CreateAssetMenu(fileName = "Cards Played This Turn Condition", menuName = "CardGame/Condition/Cards Played This Turn")]
public class CardsPlayedThisTurnCondition : ICondition
{
    [Header("필요한 카드 사용 횟수")]
    [Tooltip("이 턴에 카드를 이 횟수만큼 사용했다면 조건을 만족합니다. (현재 카드 포함)")]
    public int requiredCount = 2; // "카드를 2번 사용했다면"에 해당

    /// <summary>
    /// 조건을 검사합니다.
    /// </summary>
    /// <param name="bus">현재 실행 중인 시그널 버스</param>
    /// <returns>조건 충족 여부</returns>
    public override bool Check(SignalBus bus)
    {
        if (EventManager.Instance == null)
        {
            Debug.LogError("EventManager가 씬에 없습니다!");
            return false;
        }

        var triggeredLogs = EventManager.Instance.FindLogsThisTurn(
                    // [수정] log.Type이 SignalType인지 확인
                    log => log.Signal == SignalType.OnEffect
                );

        // 찾은 로그의 개수가 요구치(requiredCount)보다 크거나 같은지 확인합니다.
        // MainInputState에서 로그를 먼저 기록했기 때문에, 이 카드를 포함하여 계산됩니다.
        // 즉, 이 카드가 2번째로 사용된 카드라면 playedCardLogs.Count는 2가 됩니다.
        return triggeredLogs.Count >= requiredCount;
    }
}