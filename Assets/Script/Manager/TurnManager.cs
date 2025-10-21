using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;
    public int CurrentTurn;

    private void Awake()
    {
        Instance = this;
    }

    public void StartTurn()
    {
        DeckManager.Instance.BroadcastSignalToAllPiles(SignalType.onTurnStart);
        CurrentTurn += 1;
    }

    public void EndTurn()
    {
        DeckManager.Instance.BroadcastSignalToAllPiles(SignalType.OnTurnEnd);
        StartTurn(); // 다음 턴 시작
    }
    public int GetCurrentTurn()
    {
        return CurrentTurn;
    }
}
