using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting; // List<BaseData>를 사용하기 위해 추가

public class ShopSlot : MonoBehaviour
{
    public enum SlotType { Common, General, Rare, Unique }
    [Header("슬롯 설정")]
    [Tooltip("이 슬롯의 종류입니다. (기본, 일반, 희귀, 유일)")]
    public SlotType slotType;

    [Header("슬롯 설정")]
    [Tooltip("[!] 이 슬롯에 등장할 수 있는 '카드 등급' 마스크입니다. (다중 선택 가능)")]
    public Rarity allowedRarities;

    [Header("슬롯 내구도")]
    [Tooltip("이 슬롯의 현재 내구도입니다. 0이 되면 폐쇄됩니다.")]
    public int currentSlotDurability = 100;
    [Tooltip("이 슬롯이 영구적으로 폐쇄되었는지 여부입니다.")]
    public bool isSlotClosed { get; private set; } = false;

    [Header("UI 참조")]
    public Image itemIcon;
    public Image frameImage; // (가정) 등급별 테두리 이미지
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemCostText;
    //public Button buyButton;
    public Button rerollButton;
    public GameObject soldOutOverlay;
    public GameObject closedOverlay; // (권장) '폐쇄됨'을 표시할 UI
    public TextMeshProUGUI durabilityText; // (권장) 슬롯 내구도 표시용 UI

    public BaseController CurrentItem { get; private set; }
    public bool HasBeenRerolledThisTurn { get; private set; }

    public bool allowDurabilityOverdraft = false;

    private void Start()
    {
        //buyButton.onClick.AddListener(OnBuyButtonPress);
        rerollButton.onClick.AddListener(OnRerollButtonPress);
        if (ShopManager.Instance != null)
        {
            currentSlotDurability = ShopManager.Instance.maxShopDurability;
        }
        else
        {
            Debug.LogError("ShopManager 인스턴스가 없습니다!");
            currentSlotDurability = 100; // (안전 장치)
        }

        UpdateDurabilityUI(); // 시작 시 내구도 UI 업데이트
    }

    /// <summary>
    /// 턴 시작 시 호출됩니다. (책임 축소)
    /// </summary>
    public void ResetRerollFlag()
    {
        HasBeenRerolledThisTurn = false;
        rerollButton.interactable = true;
    }

    /// <summary>
    /// 리롤 완료 시 호출됩니다.
    /// </summary>
    public void MarkAsRerolled()
    {
        HasBeenRerolledThisTurn = true;
        rerollButton.interactable = false;
    }
    /// <summary>
    /// [!] ShopManager가 호출하여 이 슬롯의 '등급 마스크'를 변경합니다.
    /// </summary>
    public void ChangeAllowedRarities(Rarity newRarityMask)
    {
        this.allowedRarities = newRarityMask;

        // (가정) 등급 마스크에 따라 테두리 UI 즉시 변경
        // UpdateFrameUI(); 
    }/// <summary>
     /// [!!!] 요청 2: 슬롯의 '개별 내구도'를 소모하는 메서드
     /// </summary>
     /// <returns>소모에 성공하면 true, 내구도가 부족하면 false</returns>
    public bool TrySpendDurabilityAndCheckClosure(int cost, bool allowOverdraft)
    {
        if (isSlotClosed) return false; // 이미 닫힘

        // --- [!!!] 수정점 5: 모드 분기 ---
        if (!allowOverdraft)
        {
            // [모드 A: 엄격]
            // (기획: 40이 있어야 40 소모 가능)
            if (currentSlotDurability < cost)
            {
                return false; // 내구도 부족 (구매 불가)
            }
        }

        // [!] 내구도 소모 성공 (두 모드 공통)
        currentSlotDurability = Mathf.Max(currentSlotDurability-cost, 0); // (모드 B의 경우, -20이 될 수 있음)
        UpdateDurabilityUI();

        // [!] 소모 후 0 이하가 되었는지 확인 (두 모드 공통)
        if (currentSlotDurability <= 0)
        {
            CloseSlot(); // 슬롯 폐쇄
        }
        return true;
    }

    /// <summary>
    /// [!] 슬롯을 영구적으로 '폐쇄'합니다.
    /// </summary>
    private void CloseSlot()
    {
        isSlotClosed = true;
        SetAsEmpty(); // UI를 '판매 완료' 상태로

        // (권장) '폐쇄됨' UI 활성화
        if (closedOverlay != null)
        {
            closedOverlay.SetActive(true);
        }

        // 모든 버튼 영구 비활성화
        rerollButton.interactable = false;
    }

    // (헬퍼) 내구도 UI 업데이트
    private void UpdateDurabilityUI()
    {
        if (durabilityText != null)
        {
            durabilityText.text = isSlotClosed ? "X" : $"{currentSlotDurability} / 100";
        }
    }


    /// <summary>
    /// 아이템이 팔렸거나 비어있음을 표시합니다.
    /// </summary>
    public void SetAsEmpty()
    {
        CurrentItem = null;

        // [수정점 2] UI 초기화
        itemIcon.sprite = null; // (기본 빈 이미지로)
        itemNameText.text = "판매 완료";
        itemCostText.text = "";

        soldOutOverlay.SetActive(true);
    }
    private void OnBuyButtonPress()
    {
        if (CurrentItem == null) return;
        ShopManager.Instance.RequestBuy(CurrentItem.baseInstance);
    }

    private void OnRerollButtonPress()
    {
        ShopManager.Instance.RequestReroll(this);
    }
}