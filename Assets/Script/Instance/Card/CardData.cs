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
}