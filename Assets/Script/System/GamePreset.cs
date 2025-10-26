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
    [Serializable]
    public struct SignalActionEntry
    {
        [Tooltip("이 신호가 발생했을 때...")]
        public SignalType signal; // 예: SignalType.TurnStart

        [Tooltip("...이 액션들을 발동시킵니다.")]
        public List<BaseAction> actions; // 예: [DrawCardAction(5), GainManaAction(3)]
    }

    public string deckName;
    [TextArea] public string description;
    public Sprite icon;
    public List<CardEntry> cardEntries = new List<CardEntry>();
    public List<SignalActionEntry> gameRules;
}

[Serializable]
public class CardEntry
{
    public CardData cardData;
    public int count;
}
