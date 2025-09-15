using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
// 카드 UI 및 입력 처리 클래스
public class BaseCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI 참조")]
    public Image backgroundImage;
    public Image artworkImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI buyCostText;

    [Header("하이라이트용 (선택)")]
    public Outline highlightOutline;    // 드래그&드롭으로 연결
    public Image borderImage;           // 대체용 경계 이미지

    public CardData CardData { get; private set; }
    public BaseInstance cardInstance { get; private set; }

    public static event Action<BaseCard> OnCardClicked;
    public static event Action<BaseCard> OnCardHovered;
    public static event Action<BaseCard> OnCardUnhovered;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnCardHovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnCardUnhovered?.Invoke(this);
    }

    public void Setup(CardData data, BaseInstance instance)
    {
        CardData = data;
        cardInstance = instance;
        UpdateUI();
    }

    public void UpdateUI()
    {
        // 1) 이름
        var bus1 = new SignalBus(SignalType.NameEvaluation);
        bus1.SetSourceInfo(this.cardInstance);
        cardInstance.EvalRun(bus1);
        nameText.text = (bus1.CalcKind == CellKind.String) ? (string)bus1.CalcRaw : string.Empty;

        // 2) 설명
        var bus2 = new SignalBus(SignalType.DescriptionEvaluation);
        bus2.SetSourceInfo(this.cardInstance);
        cardInstance.EvalRun(bus2);
        descriptionText.text = (bus2.CalcKind == CellKind.String) ? (string)bus2.CalcRaw : string.Empty;

        // 3) 마나 코스트
        var bus3 = new SignalBus(SignalType.ManaCostEvaluation);
        bus3.SetSourceInfo(this.cardInstance);
        cardInstance.EvalRun(bus3);
        int mana = (bus3.CalcKind == CellKind.Int) ? (int)bus3.CalcRaw
                : (bus3.CalcKind == CellKind.Float) ? Mathf.RoundToInt((float)bus3.CalcRaw)
                : 0;
        manaCostText.text = mana.ToString();

        // 4) 구매 비용
        var bus4 = new SignalBus(SignalType.BuyCostEvaluation);
        bus4.SetSourceInfo(this.cardInstance);
        cardInstance.EvalRun(bus4);
        int buy = (bus4.CalcKind == CellKind.Int) ? (int)bus4.CalcRaw
                : (bus4.CalcKind == CellKind.Float) ? Mathf.RoundToInt((float)bus4.CalcRaw)
                : 0;
        buyCostText.text = buy.ToString();

        // // 5) 아트워크 (Sprite)
        // bus.Signal = SignalType.ArtworkEvaluation;
        // var artObj = cardInstance.Evaluate(bus);
        // if (bus.CalcKind == CellKind.Object && artObj is Sprite sp)
        //     artworkImage.sprite = sp;
        // else
        //     artworkImage.sprite = null; // 혹은 data.Artwork 같은 폴백

        // 7) 기타 UI 세팅
        if (backgroundImage != null)
            backgroundImage.color = Color.white;
    }


    // 카드 사용 호출
    public void UseCard()
    {
        if (!PlayerStats.Instance.SpendMana(CardData.manaCost))
        {
            Debug.Log("마나 부족");
            return;
        }
        cardInstance.Fire(new SignalBus(SignalType.OnUse));

        EffectCard();
        PlayedCard();
    }

    public void EffectCard()
    {
        cardInstance.Fire(new SignalBus(SignalType.OnEffect));
    }
    public void PlayedCard()
    {
        cardInstance.Fire(new SignalBus(SignalType.OnPlayed));
    }

    public void SetHighlight(bool on, Color color)
    {
        if (highlightOutline != null)
        {
            highlightOutline.enabled = on;
            highlightOutline.effectColor = color;
        }
        else if (borderImage != null)
        {
            borderImage.enabled = on;
            borderImage.color = on ? color : Color.clear;
        }
        else if (backgroundImage != null)
        {
            // 최후의 수단: 배경색 토글
            backgroundImage.color = on ? color : Color.white;
        }
    }
}