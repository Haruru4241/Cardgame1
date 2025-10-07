using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
// 카드 UI 및 입력 처리 클래스
public abstract class BaseController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Card-Specific UI")]

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    [Header("UI 참조")]
    public Image backgroundImage;
    public Image artworkImage;

    [Header("하이라이트용 (선택)")]
    public Outline highlightOutline;    // 드래그&드롭으로 연결
    public Image borderImage;           // 대체용 경계 이미지

    public BaseData baseData { get; protected set; }
    public BaseInstance baseInstance { get; protected set; }

    public static event Action<BaseController> OnEntityClicked;
    public static event Action<BaseController> OnEntityHovered;
    public static event Action<BaseController> OnEntityUnhovered;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnEntityClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEntityHovered?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnEntityUnhovered?.Invoke(this);
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

    public abstract void Setup(BaseData data, BaseInstance instance);
    public abstract void UpdateUI();
    public abstract void Use();
}