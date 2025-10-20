using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ 사용을 위해 추가

[CreateAssetMenu(fileName = "Conditional Action", menuName = "CardGame/Action/Conditional Action")]
public class ConditionalAction : BaseAction
{
    public enum ConditionLogic { All, Any } // 모든 조건(AND) / 하나라도(OR)

    [Header("조건 설정")]
    [Tooltip("조건 검사에 사용할 조건 SO 목록입니다.")]
    public List<ICondition> Conditions;

    [Tooltip("위 조건들을 어떻게 조합할지 결정합니다. (All = AND, Any = OR)")]
    public ConditionLogic LogicType = ConditionLogic.All;

    [Header("실행할 액션 목록")]
    [Tooltip("위 조건이 '참(True)'일 때 실행될 액션 SO 목록입니다.")]
    public List<BaseAction> ActionsIfTrue;

    [Tooltip("위 조건이 '거짓(False)'일 때 실행될 액션 SO 목록입니다. (선택 사항)")]
    public List<BaseAction> ActionsIfFalse;

    public override void Execute(SignalBus bus)
    {
        // 조건 목록이 비어있으면 기본적으로 '참'으로 간주할지 결정 (여기서는 참으로 간주)
        if (Conditions == null || Conditions.Count == 0)
        {
            ExecuteActions(ActionsIfTrue, bus);
            return;
        }

        bool result;
        // 조건 조합 로직 (AND 또는 OR)
        if (LogicType == ConditionLogic.All)
        {
            // 모든 조건이 참이어야 함 (AND)
            result = Conditions.All(condition => condition.Check(bus));
        }
        else // ConditionLogic.Any
        {
            // 하나라도 참이면 됨 (OR)
            result = Conditions.Any(condition => condition.Check(bus));
        }

        // 결과에 따라 적절한 액션 목록 실행
        if (result)
        {
            ExecuteActions(ActionsIfTrue, bus);
        }
        else
        {
            ExecuteActions(ActionsIfFalse, bus);
        }
    }

    /// <summary>
    /// 주어진 액션 목록을 순차적으로 실행합니다.
    /// </summary>
    private void ExecuteActions(List<BaseAction> actions, SignalBus bus)
    {
        if (actions == null || actions.Count == 0) return;

        // SequenceAction과 유사하게, 액션들을 복제하여 큐에 넣고
        // 새로운 버블이나 현재 버스에 추가하여 실행합니다.
        // 여기서는 간단하게 현재 버스에 바로 실행하는 방식으로 구현합니다. (주의: 동기적 실행)
        // 실제로는 SequenceAction처럼 처리하는 것이 더 안전할 수 있습니다.
        foreach (var action in actions)
        {
            if (action != null)
            {
                action.Execute(bus);
            }
        }
    }

    /// <summary>
    /// 이 조건부 액션에 포함된 모든 하위 액션들 (True/False 목록 모두) 중에서
    /// 특정 TokenID에 해당하는 '기본값'을 찾아 반환합니다.
    /// SequenceAction과 유사하게 작동합니다.
    /// </summary>
    public override object GetValueForTokenID(string tokenID, SignalBus bus)
    {
        object value = null;

        // 1. ActionsIfTrue 목록을 먼저 탐색합니다.
        if (ActionsIfTrue != null)
        {
            foreach (var action in ActionsIfTrue)
            {
                // 각 하위 액션에게 값을 물어봅니다.
                value = action.GetValueForTokenID(tokenID, bus);
                // 값을 찾았다면 즉시 반환합니다.
                if (value != null)
                {
                    return value;
                }
            }
        }

        // 2. ActionsIfTrue에서 못 찾았다면, ActionsIfFalse 목록을 탐색합니다.
        if (ActionsIfFalse != null)
        {
            foreach (var action in ActionsIfFalse)
            {
                // 각 하위 액션에게 값을 물어봅니다.
                value = action.GetValueForTokenID(tokenID, bus);
                // 값을 찾았다면 즉시 반환합니다.
                if (value != null)
                {
                    return value;
                }
            }
        }

        // True, False 목록 모두에서 값을 찾지 못했다면 null을 반환합니다.
        return null;
    }
}