using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// 덱 및 핸드 관리 (ZoneManager 역할도 여기서 수행)
public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    // 전용 필드 제거. 이제 소스 오브 트루스는 리스트 + 타입 딕셔너리
    [SerializeField] private List<Pile> _piles = new(); // 인스펙터에서 보기용
    private readonly Dictionary<PileType, Pile> _byType = new();

    public IReadOnlyList<Pile> AllPiles => _piles;

    public List<BaseInstance> AllInstances { get; } = new List<BaseInstance>();
    public RuleInstance Rule { get; private set; }

    [Header("핸드 UI")]
    public GameObject cardPrefab;
    public Transform handArea;
    public Transform dumpArea;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        BuildAllPiles();
    }

    // PileType enum을 순회해 전부 생성 (이름 하드코딩/일일이 new 없음)
    private void BuildAllPiles()
    {
        _piles.Clear();
        _byType.Clear();

        foreach (PileType t in System.Enum.GetValues(typeof(PileType)))
        {
            var pile = new Pile(t);
            _piles.Add(pile);
            _byType[t] = pile;
        }
    }

    public Pile GetPile(PileType type) => _byType.TryGetValue(type, out var p) ? p : null;
    public bool TryGetPile(PileType type, out Pile pile) => _byType.TryGetValue(type, out pile);

    // ----------------- 기존 기능들: 타입 검색 기반으로 동작 -----------------
    public void SetupGame(GamePreset preset)
{
    if (preset == null) { Debug.LogError("덱 프리셋 없음"); return; }

    var deck = GetPile(PileType.Deck);
    var discard = GetPile(PileType.Discard);
    var exhaust = GetPile(PileType.Exhaust);
    var hand = GetPile(PileType.Hand);
    var used = GetPile(PileType.Used);
    var rulePile = GetPile(PileType.Rule);

    if (deck == null || hand == null || discard == null)
    {
        Debug.LogError("필수 Pile(Deck/Hand/Discard) 누락");
        return;
    }

    // 초기화
    deck.Cards.Clear();
    discard.Cards.Clear();
    if (exhaust != null) exhaust.Cards.Clear();
    hand.Cards.Clear();
    if (used != null) used.Cards.Clear();
    AllInstances.Clear();

    // 덱 카드 생성
    var temp = new List<CardInstance>();
    foreach (var entry in preset.cardEntries)
    {
        for (int i = 0; i < entry.count; i++)
        {
            var ci = CreateInstanceFromData(entry.cardData, dumpArea, false);
            temp.Add(ci);
        }
    }

    // 랜덤 셔플 + 덱에 넣기
    while (temp.Count > 0)
    {
        int idx = UnityEngine.Random.Range(0, temp.Count);
        var ci = temp[idx];
        temp.RemoveAt(idx);
        deck.Add(ci);
    }

    // 룰 인스턴스
    var ruleInst = new RuleInstance(preset);
    //ruleInst.RegisterProcessor(SignalType.onTurnStart, preset.onTurnStart);
    //ruleInst.RegisterProcessor(SignalType.OnTurnEnd, preset.onTurnEnd);
    AllInstances.Add(ruleInst);
    rulePile.Add(ruleInst);

    UpdateAllCardUIs();
}

    // public void SetupGame(GamePreset preset)
    // {
    //     if (preset == null) { Debug.LogError("덱 프리셋 없음"); return; }

    //     var deck = GetPile(PileType.Deck);
    //     var discard = GetPile(PileType.Discard);
    //     var exhaust = GetPile(PileType.Exhaust);
    //     var hand = GetPile(PileType.Hand);
    //     var used = GetPile(PileType.Used);
    //     var rulePile = GetPile(PileType.Rule);

    //     if (deck == null || hand == null || discard == null)
    //     {
    //         Debug.LogError("필수 Pile(Deck/Hand/Discard) 누락");
    //         return;
    //     }

    //     deck.Cards.Clear();
    //     discard.Cards.Clear();
    //     if (exhaust != null) exhaust.Cards.Clear();
    //     hand.Cards.Clear();
    //     if (used != null) used.Cards.Clear();
    //     AllInstances.Clear();

    //     var temp = new List<CardInstance>();
    //     foreach (var entry in preset.cardEntries)
    //     {
    //         for (int i = 0; i < entry.count; i++)
    //         {
    //             var ci = new CardInstance(entry.cardData);
    //             temp.Add(ci);
    //             AllInstances.Add(ci);
    //         }
    //     }
    //     while (temp.Count > 0)
    //     {
    //         int idx = UnityEngine.Random.Range(0, temp.Count);
    //         var ci = temp[idx];
    //         var obj = Object.Instantiate(cardPrefab, dumpArea);
    //         var bc = obj.GetComponent<BaseCard>();
    //         ci.BaseCard = bc;
    //         bc.Setup((CardData)ci.BaseData, ci);
    //         obj.SetActive(false);

    //         temp.RemoveAt(idx);
    //         deck.Add(ci);
    //     }
    //     // 1) RuleInstance 단일 생성
    //     var ruleInst = new RuleInstance();
    //     ruleInst.RegisterProcessor(SignalType.onTurnStart, preset.onTurnStart);
    //     ruleInst.RegisterProcessor(SignalType.OnTurnEnd, preset.onTurnEnd);

    //     // 3) RuleInstance를 런타임 리스트와 Rule Pile에 등록
    //     AllInstances.Add(ruleInst);
    //     rulePile.Add(ruleInst);

    //     UpdateAllCardUIs();
    // }
    public CardInstance CreateInstanceFromData(CardData data, Transform parent = null, bool active = false)
    {
        if (data == null)
        {
            Debug.LogError("CardData가 null입니다.");
            return null;
        }

        var ci = new CardInstance(data);
        AllInstances.Add(ci);

        // UI 프리팹 생성
        var obj = Object.Instantiate(cardPrefab, parent ?? dumpArea);
        var bc = obj.GetComponent<BaseCard>();

        ci.BaseCard = bc;
        bc.Setup(data, ci);
        obj.SetActive(active);

        return ci;
    }


    public BaseInstance DrawOne()
    {
        var deck = GetPile(PileType.Deck);
        var discard = GetPile(PileType.Discard);
        if (deck == null || discard == null) return null;

        if (deck.Cards.Count == 0)
        {
            MigratePileCards(discard.FindAll(_ => true), deck, true);
        }

        if (deck.Cards.Count == 0)
            return null;

        var ci = deck.Cards[0];
        ReloadCustomUI(GetPile(PileType.Hand).Cards);
        return ci;
    }
    public void ReloadCustomUI(List<BaseInstance> visibleCards)
    {
        if (visibleCards == null) return;

        // 1. 현재 handArea 의 카드들 수집
        List<BaseCard> uiCards = new List<BaseCard>();
        foreach (Transform child in handArea)
            if (child.gameObject.activeSelf)
                uiCards.Add(child.GetComponent<BaseCard>());

        // 2. visibleCards 에 없는 UI → dumpArea 로 이동
        foreach (var uiCard in uiCards)
        {
            if (!visibleCards.Any(ci => ci.BaseCard == uiCard))
            {
                uiCard.transform.SetParent(dumpArea);
                uiCard.gameObject.SetActive(false);
            }
        }

        // 3. visibleCards 순서대로 handArea 에 배치
        for (int i = 0; i < visibleCards.Count; i++)
        {
            var ci = visibleCards[i];
            BaseCard bc = ci.BaseCard;
            if (bc == null) continue;

            if (bc.transform.parent != handArea)
            {
                bc.transform.SetParent(handArea);
                bc.gameObject.SetActive(true);
            }
            if (bc.transform.GetSiblingIndex() != i)
                bc.transform.SetSiblingIndex(i);
        }
    }

    public void ReloadHandUI()
    {
        var hand = GetPile(PileType.Hand);
        if (hand == null) return;

        // 1. 현재 handArea의 카드UI 목록 만들기
        List<BaseCard> uiCards = new List<BaseCard>();
        foreach (Transform child in handArea)
            if (child.gameObject.activeSelf)
                uiCards.Add(child.GetComponent<BaseCard>());

        // 2. handArea에 남아 있지만 HandPile.Cards에 없는 카드 → dumpArea로 이동
        foreach (var uiCard in uiCards)
        {
            bool exists = false;
            foreach (var ci in hand.Cards)
            {
                if (ci.BaseCard == uiCard)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                uiCard.transform.SetParent(dumpArea);
                uiCard.gameObject.SetActive(false);
            }
        }

        // 3. HandPile.Cards 순서대로 handArea에 맞게 추가/이동
        for (int i = 0; i < hand.Cards.Count; i++)
        {
            var ci = hand.Cards[i];
            BaseCard bc = ci.BaseCard;
            if (bc == null) continue;

            if (bc.transform.parent != handArea)
            {
                bc.transform.SetParent(handArea);
                bc.gameObject.SetActive(true);
            }
            if (bc.transform.GetSiblingIndex() != i)
            {
                bc.transform.SetSiblingIndex(i);
            }
        }
    }

    public void MigratePileCards(List<BaseInstance> cards, Pile toPile, bool shuffle = false)
    {
        if (toPile == null) return;

        foreach (var ci in cards)
        {
            ci.CurrentZone?.Remove(ci);
            toPile.Add(ci);
        }
        if (shuffle) toPile.Shuffle();
    }

    public void BroadcastSignalToAllPiles(SignalType signal)
    {
        var pilesSnapshot = AllPiles.ToList();
        foreach (var pile in pilesSnapshot)
        {
            var cards = pile.Cards.ToList();
            foreach (var ci in cards)
                ci.Fire(new SignalBus(signal));
        }
    }

    public void UpdateAllCardUIs()
    {
        foreach (var ci in AllInstances)
        {
            if (ci.BaseCard != null)
                ci.BaseCard.UpdateUI();
        }
    }
}
