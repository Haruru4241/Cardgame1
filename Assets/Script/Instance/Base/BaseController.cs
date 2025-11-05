using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
// 카드 UI 및 입력 처리 클래스
public abstract class BaseController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
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
    // --- [!!!] 드래그 앤 드롭을 위해 추가된 변수들 ---
    private Canvas _rootCanvas;
    private Vector3 _startDragPos;
    private Transform _originalParent;
    private bool _isDragging = false;

    protected virtual void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _image = GetComponent<Image>();
    }

    // (가정) 원활한 조작을 위해 Awake에서 참조 캐싱
    protected RectTransform _rectTransform;
    protected Image _image; // (Raycast Target 제어용)
                            // --- [!!!] IBeginDragHandler 구현 (BaseInstance에서 로직 이식) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // [!] 컨트롤러가 '데이터(Instance)'의 '상태(Zone)'를 확인
        if (baseInstance.CurrentZone == null || !baseInstance.CurrentZone.CanDragFrom(baseInstance.controller))
        {
            eventData.pointerDrag = null;
            return;
        }

        _rootCanvas = GetComponentInParent<Canvas>();
        _originalParent = transform.parent;
        _startDragPos = transform.position;

        transform.SetParent(_rootCanvas.transform, true);
        _isDragging = true;
        _image.raycastTarget = false; // 드롭 영역 감지를 위해 자신을 비활성화
    }

    // --- [!!!] IDragHandler 구현 (BaseInstance에서 로직 이식) ---
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _rectTransform.position = eventData.position; // 마우스 따라가기
    }

    // --- [!!!] IEndDragHandler 구현 (BaseInstance에서 로직 이식) ---
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;
        _image.raycastTarget = true;

        // 드롭 로직은 'Zone'이 알아서 'OnDrop'으로 처리합니다.

        // 유효한 Zone(IDropHandler가 있는) 위에 드롭되지 '않았다면'
        if (eventData.pointerEnter == null || eventData.pointerEnter.GetComponentInParent<Zone>() == null)
        {
            // [!] 컨트롤러가 'ReturnToOriginalParent'를 호출
            ReturnToOriginalParent();
        }

        _originalParent = null;
    }

    /// <summary>
    /// (헬퍼) 드래그 취소/실패 시 원래 슬롯/위치로 복귀
    /// </summary>
    public void ReturnToOriginalParent()
    {
        // [!] 컨트롤러가 자신의 Transform을 제어
        if (_originalParent != null)
        {
            transform.SetParent(_originalParent);
            transform.position = _startDragPos;
        }
    }

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