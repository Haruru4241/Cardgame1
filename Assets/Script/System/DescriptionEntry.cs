using UnityEngine;

[System.Serializable]
public class DescriptionEntry
{
    [Tooltip("설명문 템플릿에 사용할 ID (예: damage, draw)")]
    public string TokenID;

    [Tooltip("최종 값을 계산할 때 사용할 Evaluation 신호")]
    public SignalType EvaluationSignal;

    [Tooltip("값을 계산하고 전달할 때 사용할 CalcType")]
    public CalcType ValueType;
}