using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
// 덱 프리셋 SO
[CreateAssetMenu(menuName = "CardGame/DeckPreset")]
public class DeckPreset : ScriptableObject
{
    public string deckName;
    [TextArea] public string description;
    public Sprite icon;
    public List<CardEntry> cardEntries = new List<CardEntry>();
}

[Serializable]
public class CardEntry
{
    public CardData cardData;
    public int count;
}
