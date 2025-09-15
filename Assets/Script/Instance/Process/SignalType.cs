// SignalType.cs
public enum SignalType
{
    NameEvaluation,
    ManaCostEvaluation,
    BuyCostEvaluation,
    DescriptionEvaluation,
    ArtworkEvaluation,
    onTurnStart,
    OnTurnEnd,
    OnUse,
    OnSelect,
    OnUnSelect,
    OnEffect,
    OnPlayed,
    OnDraw,
    OnDrawDriver,
    OnRequirement,
    OnDiscard,
    OnExhaust,
    OnDestroy,
    DrawCount,
    // ...다른 신호들 추가 가능
}
