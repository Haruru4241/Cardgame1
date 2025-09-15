using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void StartTurn()
    {
        DeckManager.Instance.BroadcastSignalToAllPiles(SignalType.onTurnStart);
    }

    public void EndTurn()
    {
        DeckManager.Instance.BroadcastSignalToAllPiles(SignalType.OnTurnEnd);
        StartTurn(); // 다음 턴 시작
    }
}
