using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
// 덱 프리셋 SO
[CreateAssetMenu(menuName = "CardGame/GamePreset")]
public class GamePreset : ScriptableObject
{
    public string deckName;
    [TextArea] public string description;
    public Sprite icon;
    public List<CardEntry> cardEntries = new List<CardEntry>();
    
    public List<BaseAction> onTurnStart = new();
    public List<BaseAction> onTurnEnd = new();
}

[Serializable]
public class CardEntry
{
    public CardData cardData;
    public int count;
}
