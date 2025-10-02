// Assets/Script/Actions/SelectTargetFromFieldAction.cs 경로에 새로 생성하세요.

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "CardGame/Action/SelectTargetAction")]
public class SelectTargetAction : BaseAction
{
    [Tooltip("선택 가능한 대상의 조건을 정의합니다.")]
    public TargetSelector targetSelector;

    [Tooltip("선택해야 하는 대상의 수입니다.")]
    public int requiredCount = 1;

    public override void Execute(SignalBus bus)
    {
        var selectState = GameManager.Instance.SelectState as SelectState;
        if (selectState == null) return;

        // TargetSelector를 이용해 현재 필드에서 선택 가능한 후보 목록을 가져옵니다.
        var candidates = targetSelector.GetTargets(bus.GetSourceCard(), bus);

        // 선택 프로세스를 시작합니다.
        selectState.StartSelection(
            () => candidates, // 후보 목록을 전달
            requiredCount,
            (selectedList) => OnSelectionFinished(selectedList, bus), // 선택 완료 시 호출될 메서드
            bus
        );
    }

    /// <summary>
    /// 대상 선택이 완료되었을 때 호출됩니다.
    /// </summary>
    private void OnSelectionFinished(List<BaseInstance> selectedList, SignalBus bus)
    {
        bus.Target = selectedList.First();
    }
}