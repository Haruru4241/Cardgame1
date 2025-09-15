using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ReactionStackManager : MonoBehaviour
{
    public static ReactionStackManager Instance { get; private set; }
    public Stack<SignalBus> _busStack = new Stack<SignalBus>();
    public SignalBus _currentBus => _busStack.Count > 0 ? _busStack.Peek() : null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public void PushBus(SignalBus bus)
    {
        PushBuses(new[] { bus });
    }
    // 새 메서드: 여러 버스 처리 (라운드로빈으로 티켓을 쌓음)
    public void PushBuses(IEnumerable<SignalBus> buses)
    {
        if (buses == null) return;
        foreach (var bus in buses)
        {
            for (int i = 0; i < bus.PassengerCount; i++)
                _busStack.Push(bus);
        }

        StartProcessing();
    }

    public void StartProcessing()
    {
        if (_busStack.Count == 0)
        {
            GameManager.Instance._logs += "프로세스 종료\n ";
            return;
        }

        // 버스에 맨 앞 버블 1회 Next만 지시 (버블 내부 재귀로 소비)
        _currentBus.ProcessSignalAction();
    }
    public void Continue()
    {
        GameManager.Instance._logs += "티켓 만료 ";
        _busStack.Pop();
        StartProcessing();
    }
}
