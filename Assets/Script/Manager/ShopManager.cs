using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ의 Where, Select, ToList를 사용하기 위해 추가

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("글로벌 상점 설정")]
    public int maxShopDurability = 100;
    public int additionalDurabilityCost = 0;
    public bool isShopClosed = false;


    [Header("[!] 마스터 아이템 풀")]
    [Tooltip("모든 슬롯이 공유하는 '마스터' 풀입니다.")]
    public ShopPool masterCardPool; // [!] '마스터 풀' 하나만 참조

    /// <summary>
    /// [!] 요청 1: Rarity(등급)와 Durability(내구도 소모값)를
    /// 인스펙터에서 엮어주기 위한 '엔트리' 구조체입니다.
    /// </summary>
    [Header("[!] 요청 1: 등급별 내구도 소모 '엔트리'")]
    [Tooltip("Rarity 등급과 소모할 내구도 값을 엮어줍니다.")]
    public List<RarityDurabilityEntry> durabilityCostEntries;
    [System.Serializable]
    public class RarityDurabilityEntry
    {
        [Tooltip("아이템 등급 (Common, Rare 등)")]
        public Rarity rarity;
        [Tooltip("해당 등급이 소모할 내구도 값 (예: 20, 40)")]
        public int durabilityCost;
    }
    private List<BaseController> _previouslyPickedItems = new List<BaseController>();

    [Header("[!!!] 동적 슬롯 생성 설정")]
    [Tooltip("상점을 채울 'ShopSlot' 프리팹 원본입니다.")]
    public GameObject shopSlotPrefab; // [!] 슬롯 원본 프리팹

    [Header("[!!!] 동적 슬롯 생성 설정")]
    [Tooltip("[질문 1] Basic, General, Rare 등 '등급별' 슬롯 프리펩 원본 목록")]
    public List<GameObject> shopSlotPrefabs; // [!] 여러 프리펩을 받음

    [Tooltip("생성된 슬롯들이 배치될 부모 Transform(Layout Group)입니다.")]
    public Transform slotParentContainer; // [!] 슬롯이 생성될 위치

    [Tooltip("[질문 1] 게임 시작 시 생성할 '초기' 슬롯 구성 (등급별)")]
    public List<ShopSlot.SlotType> initialSlotConfiguration = new List<ShopSlot.SlotType>()
    {
        // (예시) 기획서대로 기본 6 + 일반 6 = 12개
        ShopSlot.SlotType.Common, ShopSlot.SlotType.Common, ShopSlot.SlotType.Common,
        ShopSlot.SlotType.Common, ShopSlot.SlotType.Common, ShopSlot.SlotType.Common,
        ShopSlot.SlotType.General, ShopSlot.SlotType.General, ShopSlot.SlotType.General,
        ShopSlot.SlotType.General, ShopSlot.SlotType.General, ShopSlot.SlotType.General,
    };
    [Header("런타임 슬롯 관리")]
    [Tooltip("현재 활성화된 슬롯들의 '런타임' 리스트입니다.")]
    // [!] 더 이상 인스펙터에서 수동으로 채우지 않습니다.
    private List<ShopSlot> _runtimeSlots = new List<ShopSlot>();

    // (가정) ResourceManager, ReactionStackManager 등이 존재

    private void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); }
    }
    private void Start()
    {
        InitializeShop();
    }
    public void InitializeShop()
    {
        foreach (var slot in _runtimeSlots) { Destroy(slot.gameObject); }
        _runtimeSlots.Clear();
        _previouslyPickedItems.Clear();

        if (shopSlotPrefab == null || slotParentContainer == null)
        {
            Debug.LogError("[ShopManager] ShopSlot 프리팹 또는 부모 컨테이너가 없습니다!");
            return;
        }

        // 2. '초기 설정값' 리스트를 순회하며 슬롯 생성
        foreach (ShopSlot.SlotType typeToCreate in initialSlotConfiguration)
        {
            // [!] 'CreateSlot' 함수를 호출하여 슬롯을 생성하고 리스트에 추가
            CreateSlot(typeToCreate);
        }

        OnTurnStart(); // (true: 턴 시작 최초 호출)
    }

    public ShopSlot CreateSlot(ShopSlot.SlotType type)
    {
        // 1. [!] 'slotPrefabs' 리스트에서 'type'이 일치하는 프리펩 원본을 찾습니다.
        GameObject prefabToUse = shopSlotPrefabs.Find(p => p.GetComponent<ShopSlot>().slotType == type);

        if (prefabToUse == null)
        {
            Debug.LogWarning($"[ShopManager] {type} 타입의 프리펩을 찾을 수 없습니다!");
            return null;
        }

        // 2. 프리팹 생성
        GameObject slotGO = Instantiate(prefabToUse, slotParentContainer);
        ShopSlot newSlot = slotGO.GetComponent<ShopSlot>();

        if (newSlot != null)
        {
            _runtimeSlots.Add(newSlot);
            newSlot.gameObject.name = $"ShopSlot_{_runtimeSlots.Count - 1} ({type})";
            return newSlot;
        }
        return null;
    }
    public void RemoveSlot(ShopSlot slotToRemove)
    {
        if (slotToRemove != null && _runtimeSlots.Contains(slotToRemove))
        {
            _runtimeSlots.Remove(slotToRemove);
            Destroy(slotToRemove.gameObject);
        }
    }

    public void ChangeSlotRarity(ShopSlot slotToChange, ShopSlot.SlotType newType)
    {
        if (slotToChange == null || !_runtimeSlots.Contains(slotToChange))
        {
            Debug.LogWarning("등급을 변경할 슬롯을 찾을 수 없습니다.");
            return;
        }

        // --- 1. 기존 슬롯의 '위치' 정보 저장 ---
        // (a) UI 레이아웃에서의 순서(index)
        int siblingIndex = slotToChange.transform.GetSiblingIndex();
        // (b) _runtimeSlots 리스트에서의 index
        int listIndex = _runtimeSlots.IndexOf(slotToChange);


        // --- 2. '새로운' 슬롯 생성 (CreateSlot 로직 재사용) ---

        // (a) 'newType'에 맞는 '프리펩'을 검색
        GameObject prefabToUse = shopSlotPrefabs.Find(p => p.GetComponent<ShopSlot>()?.slotType == newType);
        if (prefabToUse == null)
        {
            Debug.LogWarning($"[ShopManager] {newType} 타입의 프리펩을 찾을 수 없습니다!");
            return;
        }

        // (b) 새 프리펩 생성
        GameObject slotGO = Instantiate(prefabToUse, slotParentContainer);
        ShopSlot newSlot = slotGO.GetComponent<ShopSlot>();
        if (newSlot == null)
        {
            Destroy(slotGO); // (예외 처리)
            return;
        }

        // (c) '기존 위치'에 삽입
        newSlot.transform.SetSiblingIndex(siblingIndex);
        newSlot.gameObject.name = $"ShopSlot_{listIndex} ({newType})";

        // --- 3. '기존' 슬롯 제거 ---
        _runtimeSlots.Remove(slotToChange); // (a) 리스트에서 제거
        Destroy(slotToChange.gameObject);   // (b) 씬에서 파괴

        // --- 4. '새' 슬롯을 '리스트의 올바른 위치'에 삽입 ---
        _runtimeSlots.Insert(listIndex, newSlot);

        // --- 5. '새' 슬롯 즉시 리롤 ---
        List<BaseController> excludeList = GetCurrentExcludeList(newSlot); // (이제 newSlot 자신은 제외됨)
        excludeList.AddRange(_previouslyPickedItems);

        BaseController pickedItem = PopulateSlot(newSlot, excludeList, true);
        if (pickedItem != null)
        {
            _previouslyPickedItems.Add(pickedItem);
        }
    }
    private List<BaseController> GetCurrentExcludeList(ShopSlot self = null)
    {
        // [!] _runtimeSlots. 뒤의 줄바꿈을 제거하고 한 줄로 연결
        return _runtimeSlots.Where(s => s != self && s.CurrentItem != null)
            .Select(s => s.CurrentItem)
            .ToList();
    }

    /// <summary>
    /// [!!!] 수정된 턴 시작 로직 (중앙 통제)
    /// </summary>
    public void OnTurnStart()
    {
        if (isShopClosed) return;
        List<BaseController> pickedItemsThisRefresh = new List<BaseController>();

        foreach (var slot in _runtimeSlots)
        {
            if (slot.isSlotClosed) continue;
            slot.ResetRerollFlag();
            if (slot.CurrentItem == null)
            {
                // 2. [!!!] 수정점 3: '영구 목록' + '이번 턴 목록'을 결합
                List<BaseController> combinedExcludeList = new List<BaseController>(_previouslyPickedItems);
                combinedExcludeList.AddRange(pickedItemsThisRefresh);

                // 3. 결합된 제외 목록을 전달하여 아이템 뽑기
                BaseController pickedItem = PopulateSlot(slot, combinedExcludeList, true);

                if (pickedItem != null)
                {
                    pickedItemsThisRefresh.Add(pickedItem); // (A) 이번 턴 중복 방지
                    _previouslyPickedItems.Add(pickedItem); // (B) 영구 중복 방지
                }
            }
        }
    }

    /// <summary>
    /// [!!!] 요청하신 'GetOriginSlot' 헬퍼 함수입니다.
    /// 'allSlots' 리스트를 검색하여 해당 아이템을 현재 담고 있는 슬롯을 찾습니다.
    /// </summary>
    public ShopSlot GetOriginSlot(BaseInstance item)
    {
        if (item == null) return null;

        // LINQ를 사용해 allSlots 리스트에서
        // 'CurrentItem'이 'item' 인스턴스와 일치하는 첫 번째 슬롯을 찾습니다.
        return _runtimeSlots.FirstOrDefault(slot => slot.CurrentItem.baseInstance == item);
    }

    /// <summary>
    /// [!!!] 요청하신 대로 'BaseInstance'만 받도록 수정된 'RequestBuy'
    /// </summary>
    public void RequestBuy(BaseInstance item)
    {
        // 1. [!] 아이템을 기반으로 '원본 슬롯'을 역추적합니다.
        ShopSlot originSlot = GetOriginSlot(item);

        // 2. [!] 유효성 검사 (아이템이 슬롯에 없거나, 슬롯이 닫혔는지)
        if (originSlot == null)
        {
            Debug.LogError($"[ShopManager] 구매 요청 실패: 아이템 '{item._data.Name}'을(를) 상점 슬롯에서 찾을 수 없습니다.");
            item.controller?.ReturnToOriginalParent(); // (가정) 카드를 원래 위치로 되돌림
            return;
        }

        if (isShopClosed || originSlot.isSlotClosed || item == null)
        {
            item.controller?.ReturnToOriginalParent();
            return;
        }
        // 1a. '비용(Gold)' 계산
        var goldBus = new SignalBus(SignalType.BuyCostEvaluation, null);
        item.PrepareBus(goldBus); // (가정) item이 자신의 'PurchaseCost' 규칙을 버스에 추가
        // (가정) PlayerStats가 '전역 할인 규칙'을 버스에 추가
        int finalGoldCost = CalculationManager.Instance.Evaluate<int>(goldBus, 0);

        // 1b. '마나(Power)' 계산
        var powerBus = new SignalBus(SignalType.ManaCostEvaluation, null);
        item.PrepareBus(powerBus); // (가정) item이 자신의 'PurchasePowerCost' 규칙을 버스에 추가
        int finalPowerCost = CalculationManager.Instance.Evaluate<int>(powerBus, 0);

        // 3. 재화 확인 (비용, 마나)
        if (!PlayerStats.Instance.TrySpendGold(finalGoldCost)||!PlayerStats.Instance.TrySpendMana(finalPowerCost)||!originSlot.TrySpendDurabilityAndCheckClosure(GetDurabilityCost(item.controller.baseData.rarity), true))
        {
            item.controller?.ReturnToOriginalParent(); // [!] 구매 실패 시 복귀
            return;
        }


        // 5. [구매 확정] 신호 발사
        var purchaseBus = new SignalBus(SignalType.OnPurchase, null);
        purchaseBus.SetSourceInfo(item);
        item.Fire(purchaseBus);
    }
    /// <summary>
    /// [!!!] 요청 1: '엔트리 리스트(List)'를 '직접' 조회하여 내구도 소모량을 반환합니다.
    /// (딕셔너리를 사용하지 않으므로, 런타임 변경 사항이 즉시 반영됩니다.)
    /// </summary>
    private int GetDurabilityCost(Rarity rarity)
    {
        // List.Find()를 사용해 일치하는 첫 번째 엔트리를 찾습니다.
        RarityDurabilityEntry entry = durabilityCostEntries.Find(e => e.rarity == rarity);

        // (참고: .Find()는 struct에 대해 일치하는 항목이 없으면 default(struct)를 반환)
        // (default(RarityDurabilityEntry)의 rarity는 Rarity.None(0)이 됩니다)

        if (entry.rarity == rarity) // 일치하는 엔트리를 찾았는지 확인
        {
            return entry.durabilityCost;
        }

        // (안전 장치) 엔트리에 정의되지 않은 등급은 기본 20 소모
        Debug.LogWarning($"[ShopManager] Rarity '{rarity}'의 내구도 비용이 엔트리에 정의되지 않았습니다.");
        return 20;
    }
    /// <summary>
    /// [!!!] 수정된 개별 리롤 로직 (중앙 통제)
    /// </summary>
    public void RequestReroll(ShopSlot slot)
    {
        if (isShopClosed || slot.HasBeenRerolledThisTurn) return;
        if (PlayerStats.Instance.TrySpendMana(1))
        {
            List<BaseController> excludeList = _runtimeSlots
                .Where(s => s != slot && s.CurrentItem != null)
                .Select(s => s.CurrentItem)
                .ToList();
            excludeList.AddRange(_previouslyPickedItems); // 영구 목록 결합

            PopulateSlot(slot, excludeList, false);
            slot.MarkAsRerolled();
        }
    }
    private BaseController PopulateSlot(ShopSlot slot, List<BaseController> excludeList, bool isForced = false)
{
    if (masterCardPool == null || slot.isSlotClosed) return null;

    // 1. (데이터 뽑기) 슬롯의 마스크로 마스터 풀에서 '데이터'를 뽑음
    Rarity mask = slot.allowedRarities;
        //BaseController pickedItemData = masterCardPool.GetRandomItem(excludeList, mask);

        // 2. [!] (프리펩 생성 지시)
        //    슬롯에게 '데이터'를 전달하여 '실제 카드 인스턴스'를 생성하라고 지시
        //slot.InstantiateCard(pickedItemData);
        return null;
    //return pickedItemData; // 뽑힌 '데이터'를 반환 (중복 방지용)
}
}