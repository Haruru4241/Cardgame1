using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class BaseData : ScriptableObject
{
    public string ID;                 // 카드 고유 ID
    public string Name;
    public Rarity Rarity;                 // 카드 희귀도 (일반, 희귀 등)
    public Sprite Artwork;

    [TextArea] public string Description; // 카드 설명(룰텍스트)
    [TextArea] public string FlavorText;  // 카드 flavor text (세계관, 연출용)
    public int Cost;
    public List<TagData> Tags;
    // signal actions 등 공통 가능
    
    public CardFaction faction;           // 소속 진영/클래스(선택)
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
public class TagData {
    public string tagName;
}