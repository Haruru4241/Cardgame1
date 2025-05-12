using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// 카드 데이터 SO: 이름, 아트워크, 설명, 코스트, 동작 스크립트 참조
[CreateAssetMenu(menuName = "CardGame/CardData")]
public class CardData : ScriptableObject
{
    [Header("기본 정보")]
    public string cardID;                 // 카드 고유 ID
    public string cardName;               // 카드 이름
    public CardType cardType;             // 카드 종류 (유닛, 스펠 등)
    public Rarity rarity;                 // 카드 희귀도 (일반, 희귀 등)
    public Sprite artwork;                // 카드 아트워크
    [TextArea] public string description; // 카드 설명(룰텍스트)
    [TextArea] public string flavorText;  // 카드 flavor text (세계관, 연출용)
    public int cost; //카드 구매 비용
    public int manaCost;                  // 카드 코스트 (소모 자원)
    public int attack;                    // 공격력
    public int defense;                   // 방어력
    public int health;                    // 체력(생명력, 내구도)
    public int moveRange;                 // 이동력
    public int attackRange;               // 사거리 (1=근접, 2=원거리 등)
    public int speed;                     // 속도(턴 우선순위 등)
    public TargetType targetType;         // 타겟팅 타입(적, 아군, 전체 등)
    public List<CardSkill> activeSkills;  // 액티브 스킬 목록
    public CardFaction faction;           // 소속 진영/클래스(선택)
    public List<CardTag> tags;            // 태그/키워드 (검색, 분류용)

    [Header("액션 이벤트")]
    public List<CardAction> onDrawActions = new List<CardAction>();      // 덱에서 핸드로 이동 시
    public List<CardAction> onSelectActions = new List<CardAction>();    // 선택 시
    public List<CardAction> onUnSelectActions = new List<CardAction>();
    public List<CardAction> onRequirementActions = new List<CardAction>(); 
    public List<CardAction> onUseActions = new List<CardAction>();       // 카드의 발동 시
    public List<CardAction> onEffectActions = new List<CardAction>();    // 효과의 발동 시
    public List<CardAction> onPlayedActions = new List<CardAction>();      // 플레이 완료 후(UsedPile 이동)
    public List<CardAction> onDiscardActions = new List<CardAction>();   // 버림 더미 이동 시
    public List<CardAction> onExhaustActions = new List<CardAction>();   // 소멸(ExhaustPile 이동) 시
    public List<CardAction> onDestroyActions = new List<CardAction>();   // 파괴 시
}
public enum CardType {
    Unit, Spell, Equipment, Trap, // 필요에 따라 확장
}

public enum Rarity {
    Common, Rare, Epic, Legendary
}

public enum TargetType {
    None, Enemy, Ally, All, Self, Field, // 필요에 따라 확장
}

public enum CardFaction {
    Neutral, Human, Dragon, Mage, // 필요에 따라 확장
}

// 카드 스킬/효과/태그 등은 ScriptableObject로 설계 가능
[System.Serializable]
public class CardSkill {
    public string skillName;
    public string skillDescription;
    // public Sprite icon; 등 확장 가능
}

[System.Serializable]
public class CardEffect {
    public string effectName;
    public string effectDescription;
    // 조건, 지속 턴 등 추가
}

[System.Serializable]
public class CardTag {
    public string tagName;
}