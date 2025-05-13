using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ReactionEntry
{
    public SignalType Signal;
    public Processor  Proc;

    public ReactionEntry(SignalType signal, Processor proc)
    {
        Signal = signal;
        Proc   = proc;
    }
}

public class ReactionStackManager : MonoBehaviour
{
    public static ReactionStackManager Instance { get; private set; }

    // 배치 단위로 보관하는 LIFO 스택
    private readonly Stack<List<ReactionEntry>> _stack = new Stack<List<ReactionEntry>>();
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Fire(signal) 시 호출합니다.
    /// 리스트(reactingProcs) 내부 순서는 유지(FIFO),
    /// 그러나 배치 단위 자체는 나중 푸시된 것이 먼저 처리(LIFO).
    /// </summary>
    public void PushReactions(SignalType signal, List<Processor> reactingProcs)
    {
        // 리스트를 한 번에 ReactionEntry 배치로 변환
        var batch = reactingProcs
            .Select(proc => new ReactionEntry(signal, proc))
            .ToList();

        // 배치 단위로 스택에 푸시
        _stack.Push(batch);

        // 항상 즉시 처리 시작
        ProcessNext();
    }

    /// <summary>
    /// 스택에서 배치 하나를 Pop하여 순차 처리하고,
    /// 완료 시 다음 배치로 계속 이어갑니다.
    /// </summary>
    private void ProcessNext()
    {
        if (_stack.Count == 0)
        {
            DeckManager.Instance.UpdateAllCardUIs();
            return;
        }
        var batch = _stack.Pop();
        ProcessBatch(batch, ProcessNext);
    }

    /// <summary>
    /// 한 배치 안의 ReactionEntry들을 FIFO 순서로 실행
    /// </summary>
    private void ProcessBatch(List<ReactionEntry> batch, Action onBatchDone)
    {
        int index = 0;
        // 재귀 콜백
        Action next = null;
        next = () =>
        {
            if (index >= batch.Count)
            {
                onBatchDone();
                return;
            }
            var entry = batch[index++];
            entry.Proc.ProcessSignal(entry.Signal, next);
        };
        // 첫 실행
        next();
    }
}
