using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public int cardsPerTurn = 5;

    private void Awake()
    {
        Instance = this;
    }

    public void StartTurn()
    {
        PlayerStats.Instance.StartTurn();
        DeckManager.Instance.DrawCards(cardsPerTurn);
    }

    public void EndTurn()
    {
        DeckManager.Instance.BroadcastSignalToAllPiles(SignalType.OnTurnEnd);
        DeckManager.Instance.EndTurn();
        StartTurn(); // 다음 턴 시작
    }
}
