using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 카드 데이터 SO: 이름, 아트워크, 설명, 코스트, 동작 스크립트 참조
[CreateAssetMenu(menuName = "CardGame/CardData")]
public class CardData : BaseData
{
    [Header("기본 정보")]
    
    public CardType cardType;             // 카드 종류 (유닛, 스펠 등)
    public int manaCost;                  // 카드 코스트 (소모 자원)
    public int attack;                    // 공격력
    public int defense;                   // 방어력
    public int health;                    // 체력(생명력, 내구도)
    public int moveRange;                 // 이동력
    public int attackRange;               // 사거리 (1=근접, 2=원거리 등)
    public int speed;                     // 속도(턴 우선순위 등)
    public TargetType targetType;         // 타겟팅 타입(적, 아군, 전체 등)
    public List<CardSkill> activeSkills;  // 액티브 스킬 목록

    [Header("액션 이벤트")]
    public List<BaseAction> onDrawActions = new List<BaseAction>();      // 덱에서 핸드로 이동 시
    public List<BaseAction> onSelectActions = new List<BaseAction>();    // 선택 시
    public List<BaseAction> onUnSelectActions = new List<BaseAction>();
    public List<BaseAction> onRequirementActions = new List<BaseAction>(); 
    public List<BaseAction> onUseActions = new List<BaseAction>();       // 카드의 발동 시
    public List<BaseAction> onEffectActions = new List<BaseAction>();    // 효과의 발동 시
    public List<BaseAction> onPlayedActions = new List<BaseAction>();      // 플레이 완료 후(UsedPile 이동)
    public List<BaseAction> onDiscardActions = new List<BaseAction>();   // 버림 더미 이동 시
    public List<BaseAction> onExhaustActions = new List<BaseAction>();   // 소멸(ExhaustPile 이동) 시
    public List<BaseAction> onDestroyActions = new List<BaseAction>();   // 파괴 시
}