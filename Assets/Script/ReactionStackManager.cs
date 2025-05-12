// ReactionStackManager.cs
using System;
using System.Collections.Generic;
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

    // LIFO 배치, FIFO 항목
    public readonly List<ReactionEntry> _queue = new List<ReactionEntry>();
    // 지금 처리 중인 엔트리를 저장할 필드
    private ReactionEntry _currentEntry;

    // 외부에서 참조할 수 있도록 프로퍼티 제공
    public Processor CurrentProcessor => _currentEntry.Proc;

    // 단일 매니저 토큰
    public Token _managerToken;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UploadManagerToken(object source, Action callback)
    {
        GameManager.Instance._logs += $"{_queue.Count}: 업로드매니저토큰 ";
        _managerToken = new Token(source, callback);
    }

    public void ConsumeManagerToken(object source)
    {
        GameManager.Instance._logs += $"{_queue.Count}: 컨슘매니저토큰 ";
        _managerToken.InvokeIfSource(source);
    }
        public Token UpdateManagerToken(object source)
    {
        GameManager.Instance._logs += " 핸들러업데이트 ";
        _managerToken.UpdateToken(source);
        return _managerToken;
    }

    public void PushReactions(SignalType signal, List<Processor> reactingProcs)
    {
        // 1) reactingProcs → ReactionEntry 배치
        var batch = reactingProcs
            .ConvertAll(p => new ReactionEntry(signal, p));
        GameManager.Instance._logs += $"{batch.Count}: 추가 배치 수 ";

        // 2) 기존 큐 뒤에 붙이고, 큐 전체를 배치 앞에 덮어쓰기
        batch.AddRange(_queue);
        _queue.Clear();
        _queue.AddRange(batch);

        // 3) 즉시 처리 시작
        var next = _queue[0].Proc;
        UploadManagerToken(next, ProcessNext);
        ConsumeManagerToken(next);
    }

    public void ProcessNext()
    {
        GameManager.Instance._logs += $"{_queue.Count}: 프로세스넥스트 ";

        if (_queue.Count == 0)
        {
            GameManager.Instance._logs += " 큐 종료 ";
            DeckManager.Instance.UpdateAllCardUIs();
            return;
        }

        // 1) FIFO로 꺼내 처리
        _currentEntry = _queue[0];

        // 2) 다음 호출 예약
        UploadManagerToken(_currentEntry.Proc, () =>
    {
        GameManager.Instance._logs += $"{_queue.Count}: 프로세스 종료 ";
        // 완료 시점: 진행 중 항목 제거
        _queue.RemoveAt(0);
        ProcessNext();
    });


        // 3) 해당 프로세서에 신호 전달
        _currentEntry.Proc.ProcessSignal(_currentEntry.Signal);
    }
}
