// Assets/Script/Actions/SelectTargetAction.cs (이 코드로 교체하세요)

using UnityEngine;
using System.Collections.Generic;
using System; // Func 사용을 위해 추가

[CreateAssetMenu(menuName = "CardGame/Action/Select Target Action")]
public class SelectTargetAction : BaseAction
{
    [Header("1. 어떤 방식으로?")]
    [Tooltip("대상을 선택할 방식을 결정합니다. (예: 조준선 방식)")]
    public SelectionMode selectionMode;
    
    [Header("2. 어떤 대상을?")]
    [Tooltip("선택 가능한 대상의 조건을 정의합니다.")]
    public TargetSelector targetSelector;

    [Header("3. 몇 개를?")]
    [Tooltip("선택해야 하는 대상의 수입니다.")]
    public int requiredCount = 1;

    public override void Execute(SignalBus bus)
    {
        // 1. GameManager에서 새로운 InteractionState를 가져옵니다.
        var interactionState = GameManager.Instance.InteractionState as InteractionState;
        if (interactionState == null)
        {
            Debug.LogError("GameManager에 InteractionState가 설정되어 있지 않습니다!");
            return;
        }

        // 2. TargetSelector를 호출하는 '방법(Func)'을 만듭니다. (지연 실행을 위해)
        Func<List<BaseInstance>> getCandidatesFunc = () => targetSelector.GetTargets(bus.GetSourceCard(), bus);

        // 3. InteractionState에 모든 정보를 넘겨주고 선택 프로세스 시작을 요청합니다.
        interactionState.StartSelection(
            selectionMode,      // 어떻게 선택할지 (SO)
            getCandidatesFunc,  // 누가 후보인지 (함수)
            requiredCount,      // 몇 명을 뽑아야 하는지
            (selectedList) => OnSelectionFinished(selectedList, bus), // 끝나면 뭘 할지 (콜백)
            bus                 // 토큰 관리를 위한 버스 전달
        );
    }

    /// <summary>
    /// 대상 선택이 완료되었을 때 호출되는 콜백 메서드입니다.
    /// </summary>
    private void OnSelectionFinished(List<BaseInstance> selectedList, SignalBus bus)
    {
        if (selectedList != null && selectedList.Count > 0)
        {
            // 선택된 대상 목록을 'TargetList' 타입의 Cell로 만들어 버스에 추가합니다.
            bus.AddCalculationStep(new Cell(CalcType.TargetList, CalcOp.Set, selectedList));
            
            // (디버그용 로그)
            // GameManager.Instance._logs += $"타겟설정 ";
            // foreach (var target in selectedList)
            // {
            //     GameManager.Instance._logs += $"{target._data.Name} ";
            // }
        }
    }
}