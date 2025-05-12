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
    public CardInstance cardInstance { get; private set; }

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

    [ContextMenu("▶ Show Signal–Processor Bindings")]
    public void DebugLogBindings()
    {
        var bindings = cardInstance.GetSignalProcessorBindings();
        if (bindings.Count == 0)
        {
            return;
        }

        foreach (var b in bindings)
        {
            Debug.Log($"Signal: {b.Signal}, Processor: \"{b.ProcessorName}\"");
        }
    }

    public void Setup(CardData data, CardInstance instance)
    {
        CardData = data;
        cardInstance = instance;
        UpdateUI();
    }

    public void UpdateUI()
    {
        nameText.text = cardInstance.Evaluate<string>(SignalType.NameEvaluation);
        descriptionText.text = cardInstance.Evaluate<string>(SignalType.DescriptionEvaluation);
        manaCostText.text = cardInstance.Evaluate<int>(SignalType.ManaCostEvaluation).ToString();
        buyCostText.text = cardInstance.Evaluate<int>(SignalType.BuyCostEvaluation).ToString();
        artworkImage.sprite = cardInstance.Evaluate<Sprite>(SignalType.ArtworkEvaluation);

        if (backgroundImage != null)
            backgroundImage.color = Color.white;
    }


    // public void OnPointerClick(PointerEventData eventData)
    // {
    //     // 선택 동작
    //     //cardInstance.Fire(SignalType.OnUse);
    //     InputManager.Instance.OnCardClicked(this);
    // }
    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     //cardInstance.Fire(SignalType.OnSelect);
    //     InputManager.Instance.OnCardHovered(this);
    // }

    // public void OnPointerExit(PointerEventData eventData)
    // {
    //     //cardInstance.Fire(SignalType.OnUnSelect);
    //     InputManager.Instance.OnCardUnhovered(this);
    // }

    // 카드 사용 호출
    public void UseCard()
    {
        if (!PlayerStats.Instance.SpendMana(CardData.manaCost))
        {
            Debug.Log("마나 부족");
            return;
        }
        cardInstance.Fire(SignalType.OnUse);

        EffectCard();
        PlayedCard();
    }
    
    public void EffectCard()
    {
        cardInstance.Fire(SignalType.OnEffect);
    }
    public void PlayedCard()
    {
        cardInstance.Fire(SignalType.OnPlayed);
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