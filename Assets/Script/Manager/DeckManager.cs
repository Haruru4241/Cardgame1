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


    [System.Serializable]
    public class PileSignalEntry
    {
        public PileType pileType;
        public SignalType changeSignal;
    }
    [Header("파일-신호 설정")]
    [Tooltip("게임에서 사용할 파일들과 각 파일이 변경될 때 방송할 신호를 여기에 등록합니다.")]
    public List<PileSignalEntry> pileSignalEntries = new List<PileSignalEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }
    // 수정된 파일 생성 메소드
    private void BuildAllPiles()
    {
        _piles.Clear();
        _byType.Clear();

        // Enum을 순회하는 대신, 인스펙터에 등록된 엔트리 리스트를 순회합니다.
        foreach (var entry in pileSignalEntries)
        {
            // 각 엔트리의 정보(파일 타입, 신호 타입)를 사용하여 파일을 생성합니다.
            var pile = new Pile(entry.pileType, entry.changeSignal);
            _piles.Add(pile);
            _byType[entry.pileType] = pile;
        }
    }

    public Pile GetPile(PileType type) => _byType.TryGetValue(type, out var p) ? p : null;
    public bool TryGetPile(PileType type, out Pile pile) => _byType.TryGetValue(type, out pile);

    // ----------------- 기존 기능들: 타입 검색 기반으로 동작 -----------------
    public void SetupGame(GamePreset preset)
    {
        BuildAllPiles();

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
        AllInstances.Add(ruleInst);
        rulePile.Add(ruleInst);

        //UpdateAllCardUIs();
        ReloadCustomUI(GetPile(PileType.Hand).Cards);
    }

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

    // public void BroadcastSignalToAllPiles(SignalType signal)
    // {
    //     var pilesSnapshot = AllPiles.ToList();
    //     foreach (var pile in pilesSnapshot)
    //     {
    //         // var cards = pile.Cards.ToList();
    //         // var busesToPush = new List<SignalBus>();

    //         // foreach (var ci in cards)
    //         // {
    //         //     busesToPush.Add(ci.PrepareBus(new SignalBus(signal)));

    //         // }

    //         // // 3. 모든 준비가 끝난 후, 한 번에 출발시킵니다.
    //         // if (busesToPush.Count > 0)
    //         //     ReactionStackManager.Instance.PushBuses(busesToPush); // PushSequence 사용 권장
    //         var cards = pile.Cards.ToList();
    //         foreach (var ci in cards)
    //             ci.Fire(new SignalBus(signal));
    //     }
    // }
    public void BroadcastSignalToAllPiles(SignalType signal)
{
    // 1. 앞으로 실행할 모든 버스를 담을 '단 하나의' 리스트를 루프 시작 전에 만듭니다.
    var allBusesToPush = new List<SignalBus>();

    // 2. 모든 파일을 안전하게 순회합니다.
    var pilesSnapshot = AllPiles.ToList();
    foreach (var pile in pilesSnapshot)
    {
        var cards = pile.Cards.ToList();
        foreach (var ci in cards)
        {
            // 3. 'fire'가 아닌 'GetPreparedBus'로 준비된 버스를 가져와
            //    하나의 거대한 리스트에 모두 담습니다.
            var preparedBus = ci.PrepareBus(new SignalBus(signal));
            if (preparedBus != null)
            {
                allBusesToPush.Add(preparedBus);
            }
        }
    }

    // 4. 모든 루프가 끝난 후, 수집된 모든 버스를 단 한 번에, 순서를 보장하여 실행시킵니다.
    if (allBusesToPush.Count > 0)
    {
        // 여러 버스를 순서대로 처리해야 하므로 PushSequence를 사용하는 것이 가장 안전합니다.
        ReactionStackManager.Instance.PushBuses(allBusesToPush);
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
