using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ReactionEntry
{
    public SignalType Signal;
    public Processor Proc;

    public ReactionEntry(SignalType signal, Processor proc)
    {
        Signal = signal;
        Proc = proc;
    }
}

public class ReactionStackManager : MonoBehaviour
{
    public static ReactionStackManager Instance { get; private set; }
    public readonly List<ReactionEntry> _queue = new List<ReactionEntry>();
    public ReactionEntry _currentEntry;

    // 외부에서 참조할 수 있도록 프로퍼티 제공
    public Processor CurrentProcessor => _currentEntry.Proc;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PushReactions(SignalType signal, List<Processor> reactingProcs)
    {
        // 새로 들어온 배치를 큐 앞에 삽입 (LIFO 배치, 내부는 FIFO 유지)
        var batch = reactingProcs
            .Select(proc => new ReactionEntry(signal, proc))
            .ToList();
        _queue.InsertRange(0, batch);
        GameManager.Instance._logs += string.Format(" {0} 액션 시작 현재 {1} 추기 {2}", signal, _queue.Count, batch.Count);

        // 항상 바로 처리 시작
        ProcessNext();
    }

    public void ProcessNext()
    {
        if (_queue.Count == 0)
        {
            GameManager.Instance._logs += string.Format(" 프로세스 종료 ");
            DeckManager.Instance.UpdateAllCardUIs();
            return;
        }
        
        GameManager.Instance._logs += string.Format(" {0} 남은 현재 큐 ", _queue.Count);

        // 현재 작업은 큐의 맨 앞
        _currentEntry = _queue[0];

        // 완료 콜백 안에서만 제거하고, 그다음 ProcessNext 호출
        _currentEntry.Proc.ProcessSignal(_currentEntry.Signal);
    }
}
