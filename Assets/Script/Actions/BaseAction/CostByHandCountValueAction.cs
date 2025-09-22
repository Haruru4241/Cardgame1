using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Actions/Value/Provider/Count From TargetSelector")]
public class CountFromTargetSelectorValueProvider : ValueAction
{
    [Header("대상 설정")]
    [Tooltip("개수를 셀 대상을 찾는 TargetSelector를 여기에 연결합니다.")]
    public TargetSelector targetSelector;

    public override object GetValue(SignalBus bus)
    {
        if (targetSelector == null)
        {
            Debug.LogWarning($"Value provider '{this.name}'에 TargetSelector가 설정되지 않았습니다.");
            return 0;
        }

        // TargetSelector에게 대상 찾기를 위임하고, 그 결과 목록의 개수를 반환합니다.
        return targetSelector.GetTargets(bus.SourceObject).Count;
    }
}