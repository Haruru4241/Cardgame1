using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 카드의 현재 UI/UX 상호작용 상태를 나타내는 조건 플래그입니다. (비트마스크)
/// </summary>
[Flags]
public enum ContextCondition
{
    None = 0,

    /// <summary>
    /// 드래그나 선택 등 다른 상호작용 없이, 단순히 대기/관찰 중인 기본 상태입니다.
    /// (예: 손패나 필드에 가만히 있을 때)
    /// </summary>
    IsIdle = 1 << 0,

    /// <summary>
    /// 플레이어가 카드를 사용하기 위해 드래그/조준 중인 상태입니다.
    /// </summary>
    IsTargeting = 1 << 1,

    /// <summary>
    /// '발견'이나 '선택' 효과처럼, 플레이가 아닌 선택을 기다리는 상태입니다.
    /// </summary>
    IsChoicePending = 1 << 2,

    /// <summary>
    /// 게임 외부의 카드 도감(아카이브)에서 카드를 보고 있는 상태입니다.
    /// </summary>
    IsInArchive = 1 << 3,
}

/// <summary>
/// 값 계산에 영향을 미치는 Processor를 어떤 대상으로부터 수집할지 정의하는 플래그입니다. (비트마스크)
/// </summary>
[Flags]
public enum CollectorType
{
    None = 0,
    
    /// <summary>
    /// 값 계산의 주체가 되는 인스턴스 자신 (예: 카드 자신)
    /// </summary>
    Source = 1 << 0,

    /// <summary>
    /// 주체를 소유한 인스턴스 (예: 플레이어)
    /// </summary>
    Owner = 1 << 1,

    /// <summary>
    /// 주체의 효과가 적용될 대상 인스턴스
    /// </summary>
    Target = 1 << 2,

    /// <summary>
    /// 상황과 관계없이 항상 영향을 미치는 전역 인스턴스 (예: 유물)
    /// </summary>
    Global = 1 << 3,

    /// <summary>
    /// 주체가 현재 위치한 존(Zone) 인스턴스 (예: 손패 존)
    /// </summary>
    Zone = 1 << 4,
}

/// <summary>
/// "어떤 조건일 때, 무엇을 수집할지"를 정의하는 하나의 규칙입니다.
/// </summary>
[System.Serializable]
public class ContextRuleEntry
{
    [Tooltip("이 규칙에 대한 설명 (인스펙터용)")]
    public string Description;

    [Header("IF: 다음 조건들을 '모두' 만족하면")]
    [Tooltip("규칙이 발동하기 위해 필요한 현재 UI/UX 상태 조건입니다.")]
    public ContextCondition ConditionMask;

    [Tooltip("규칙이 발동하기 위해 카드가 위치해야 하는 존(Zone)입니다. (None은 모든 존 허용)")]
    public PileType RequiredZoneMask;

    [Header("THEN: 다음 대상들을 '모두' 수집한다")]
    [Tooltip("위 조건이 맞을 때 수집할 대상들의 목록입니다.")]
    public CollectorType CollectorMask;
    
    [Header("ETC")]
    [Tooltip("여러 규칙이 동시에 만족될 경우, 이 숫자가 높은 규칙이 우선 적용됩니다.")]
    public int Priority = 10;
}


/// <summary>
/// 게임의 모든 동적 값 계산 상황 판단 규칙을 담고 있는 단 하나의 SO입니다.
/// </summary>
[CreateAssetMenu(fileName = "ContextRuleSet", menuName = "CardGame/System/Context Rule Set")]
public class ContextRuleSet : ScriptableObject
{
    [Tooltip("모든 상황 판단 규칙 엔트리 목록입니다. 우선순위(Priority)가 높은 순으로 적용됩니다.")]
    public List<ContextRuleEntry> Rules;

    [Header("기본 규칙")]
    [Tooltip("어떤 규칙에도 해당하지 않을 경우, 사용할 기본 수집 대상입니다.")]
    public CollectorType DefaultCollectors = CollectorType.Source;
}