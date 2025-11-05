using UnityEngine;

[CreateAssetMenu(fileName = "DeckCountCondition", menuName = "CardGame/Conditions/Deck Count Condition")]
public class DeckCountCondition : ICondition
{
    public enum ComparisonType { LessThan, EqualTo, GreaterThan }

    [Tooltip("비교할 덱의 종류입니다.")]
    public ZoneType targetDeck = ZoneType.Deck; // 예: 플레이어 덱

    [Tooltip("비교 방식입니다. (예: 덱 매수가 count보다 적은가?)")]
    public ComparisonType comparison = ComparisonType.EqualTo;

    [Tooltip("비교할 카드 매수입니다.")]
    public int count = 0;

    public override bool Check(SignalBus bus)
    {
        // DeckManager 등에서 해당 덱의 실제 카드 수를 가져옵니다.
        int currentDeckCount = DeckManager.Instance.GetPile(targetDeck).Cards.Count;

        switch (comparison)
        {
            case ComparisonType.LessThan:
                return currentDeckCount < count;
            case ComparisonType.EqualTo:
                return currentDeckCount == count;
            case ComparisonType.GreaterThan:
                return currentDeckCount > count;
            default:
                return false;
        }
    }
}