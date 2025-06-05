using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReactionStackManager : MonoBehaviour
{
    public static ReactionStackManager Instance { get; private set; }
    public Stack<SignalBus> _busStack = new Stack<SignalBus>();
    public SignalBus _currentBus;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PushBus(SignalBus bus)
    {
        if (bus == null || !bus.HasPassenger())
            return;

        for (int i = 0; i < bus.PassengerCount; i++)
        {
            _busStack.Push(bus);
        }

        // 처리 루프 시작 (이미 돌고 있으면 StartProcessing()에서 걸러짐)
        StartProcessing();
    }
    public void StartProcessing()
    {
        if (_busStack.Count == 0)
        {
            GameManager.Instance._logs += string.Format(" 프로세스 종료 ");
            DeckManager.Instance.UpdateAllCardUIs();
            return;
        }

        // 현재 작업은 큐의 맨 앞
        _currentBus = _busStack.Peek();

        // 완료 콜백 안에서만 제거하고, 그다음 ProcessNext 호출
        _currentBus.PeekNextPassenger().ProcessSignal(_currentBus);
    }
}
