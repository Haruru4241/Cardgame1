using System.Collections.Generic;
using UnityEngine;
using System.Linq;
// 덱 및 핸드 관리
public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }


    public Pile DeckPile { get; private set; }
    public Pile DiscardPile { get; private set; }
    public Pile DestroyPile { get; private set; }
    public Pile ExhaustPile { get; private set; }
    public Pile HandPile { get; private set; }
    public Pile UsedPile { get; private set; }
    public List<Pile> AllPiles
            => new List<Pile> { DeckPile, DiscardPile, ExhaustPile, DestroyPile, HandPile, UsedPile };
    public List<CardInstance> AllCardInstances { get; } = new List<CardInstance>();

    [Header("핸드 UI")]
    public GameObject cardPrefab;
    public Transform handArea;
    public Transform dumpArea;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        DeckPile = new Pile("Deck");
        DiscardPile = new Pile("Discard");
        ExhaustPile = new Pile("Exhaust");
        HandPile = new Pile("Hand");
        DestroyPile = new Pile("Destroy");
        UsedPile = new Pile("Used");
    }

    public void SetupDeck(DeckPreset preset)
    {
        if (preset == null) { Debug.LogError("덱 프리셋 없음"); return; }

        DeckPile.Cards.Clear();
        DiscardPile.Cards.Clear();
        ExhaustPile.Cards.Clear();
        HandPile.Cards.Clear();
        UsedPile.Cards.Clear();
        AllCardInstances.Clear();

        var temp = new List<CardInstance>();
        foreach (var entry in preset.cardEntries)
        {
            for (int i = 0; i < entry.count; i++)
            {
                var ci = new CardInstance(entry.cardData);
                temp.Add(ci);
                AllCardInstances.Add(ci);
            }
        }
        while (temp.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, temp.Count);
            var ci = temp[idx];
            var obj = Instantiate(cardPrefab, dumpArea);
            var bc = obj.GetComponent<BaseCard>();
            ci.BaseCard = bc;
            bc.Setup(ci.CardData, ci);
            obj.SetActive(false);

            temp.RemoveAt(idx);
            DeckPile.Add(ci);
        }
    }

    // 카드 뽑기 & UI 스폰
    public int DrawCards(int count)
    {
        GameManager.Instance._logs += " 드로우 시작 ";
        int drawn = 0;
        for (int i = 0; i < count; i++)
        {
            if (DeckPile.Cards.Count == 0)
            {
                MigratePileCards(DiscardPile.FindAll(c => true), DeckPile, true);
            }        // **변경**: 덱 비었으면 즉시 재활용

            if (DeckPile.Cards.Count == 0)
                break;

            var ci = DeckPile.Cards[0];

            if (ci.BaseCard != null)
            {
                ci.BaseCard.cardInstance.Fire(SignalType.OnDraw);
            }

            drawn++;
        }
        GameManager.Instance._logs += " \n\n ";
        ReloadHandUI();
        return drawn;
    }

    public void EndTurn()
    {
        // 사용된 카드 모두 버림
        MigratePileCards(UsedPile.FindAll(c => true), DiscardPile);

        // 핸드 카드 모두 버림
        MigratePileCards(HandPile.FindAll(c => true), DiscardPile);

        // 핸드 UI 리로딩
        ReloadHandUI();
    }

    public void ReloadHandUI()
    {
        // 1. 현재 handArea의 카드UI 목록 만들기
        List<BaseCard> uiCards = new List<BaseCard>();
        foreach (Transform child in handArea)
            if (child.gameObject.activeSelf)
                uiCards.Add(child.GetComponent<BaseCard>());

        // 2. handArea에 남아 있지만 HandPile.Cards에 없는 카드 → dumpArea로 이동
        foreach (var uiCard in uiCards)
        {
            bool exists = false;
            foreach (var ci in HandPile.Cards)
            {
                if (ci.BaseCard == uiCard)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                // handArea에만 남은 카드 제거
                uiCard.transform.SetParent(dumpArea);
                uiCard.gameObject.SetActive(false);
            }
        }

        // 3. HandPile.Cards 순서대로 handArea에 맞게 추가/이동
        for (int i = 0; i < HandPile.Cards.Count; i++)
        {
            var ci = HandPile.Cards[i];
            BaseCard bc = ci.BaseCard;
            if (bc == null)
                continue;

            // handArea에 없으면 이동/추가
            if (bc.transform.parent != handArea)
            {
                bc.transform.SetParent(handArea);
                bc.gameObject.SetActive(true);
            }
            // 올바른 위치에 있는지 확인하고 정렬
            if (bc.transform.GetSiblingIndex() != i)
            {
                bc.transform.SetSiblingIndex(i);
            }
        }
    }

    public void MigratePileCards(List<CardInstance> cards, Pile toPile, bool shuffle = false)
    {
        foreach (var ci in cards)
        {
            ci.CurrentPile?.Remove(ci); // 현재 속한 Pile에서 제거
            toPile.Add(ci);             // 목적지 Pile로 추가
        }
        if (shuffle) toPile.Shuffle();
    }
    public void BroadcastSignalToAllPiles(SignalType signal)
    {
        // Pile 리스트 복사(안정성)
        var pilesSnapshot = AllPiles.ToList();

        foreach (var pile in pilesSnapshot)
        {
            // Pile 내 카드 리스트 복사
            var cards = pile.Cards.ToList();

            foreach (var ci in cards)
                ci.Fire(signal);
        }
    }
    public void UpdateAllCardUIs()
    {
        foreach (var ci in AllCardInstances)
        {
            if (ci.BaseCard != null)
                ci.BaseCard.UpdateUI();
        }
    }
}

