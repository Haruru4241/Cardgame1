// Assets/Script/System/SelectionModes/SelectionMode.cs (이 코드로 교체하세요)

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 '선택 모드' SO의 기반이 되는 추상 클래스입니다.
/// 각 모드는 이제 스스로 Enter, Update, Exit 로직을 정의해야 합니다.
/// </summary>
public abstract class SelectionMode : ScriptableObject
{
    /// <summary>
    /// 선택 모드가 시작될 때 호출됩니다.
    /// </summary>
    /// <param name="state">이 모드를 제어하는 InteractionState</param>
    public abstract void OnEnter(InteractionState state);

    /// <summary>
    /// 선택 모드가 활성화된 동안 매 프레임 호출됩니다.
    /// </summary>
    /// <param name="state">이 모드를 제어하는 InteractionState</param>
    public abstract void OnUpdate(InteractionState state);

    /// <summary>
    /// 선택 모드가 종료될 때 호출됩니다.
    /// </summary>
    /// <param name="state">이 모드를 제어하는 InteractionState</param>
    public abstract void OnExit(InteractionState state);
    
}