using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 적의 원본 데이터를 정의하는 ScriptableObject입니다.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy Data", menuName = "CardGame/Enemy Data")]
public class EnemyData : BaseData
{
    [Header("Enemy Stats")]
    public int Health; // 적의 최대 체력
    public int Damage;

    public int defense;                   // 방어력
    public int moveRange;                 // 이동력
    public int attackRange;               // 사거리 (1=근접, 2=원거리 등)
    public int speed;                     // 속도(턴 우선순위 등)
    public TargetType targetType;         // 타겟팅 타입(적, 아군, 전체 등)
    public List<CardSkill> activeSkills;  // 액티브 스킬 목록
}