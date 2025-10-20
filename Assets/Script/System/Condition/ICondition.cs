using UnityEngine;

/// <summary>
/// 모든 조건 SO의 기반이 되는 인터페이스(또는 추상 클래스)입니다.
/// 현재 게임 상태(SignalBus)를 기준으로 조건 충족 여부를 판단합니다.
/// </summary>
public abstract class ICondition : ScriptableObject
{
    /// <summary>
    /// 현재 게임 상황(bus)이 이 조건을 만족하는지 검사합니다.
    /// </summary>
    /// <param name="bus">현재 액션 실행 컨텍스트를 담고 있는 SignalBus</param>
    /// <returns>조건 만족 시 true, 아니면 false</returns>
    public abstract bool Check(SignalBus bus);
}