// ───────────────────────────────────────────────────────────
// 파일명: Processor.cs
// 목적: SignalHandler 제거, BaseAction 기반으로만 동작
//  - 시그널별 액션 등록/조회
//  - Fire에서 버블 생성 시 사용할 큐(BuildActionQueue) 제공
// ───────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;

public class Processor
{
    public string       SourceName { get; }
    public bool         IsBase     { get; }
    public BaseInstance Owner      { get; }
    public BaseInstance Source     { get; }

    private readonly Dictionary<SignalType, List<BaseAction>> _actionHandlers
        = new Dictionary<SignalType, List<BaseAction>>();

    public Processor(string sourceName, bool isBase, BaseInstance owner, BaseInstance source = null)
    {
        SourceName = sourceName;
        IsBase     = isBase;
        Owner      = owner;
        Source     = source ?? owner;
    }

    // 시그널별 액션 등록
    public void RegisterAction(SignalType signal, BaseAction action)
    {
        if (action == null) return;
        if (!_actionHandlers.TryGetValue(signal, out var list))
        {
            list = new List<BaseAction>();
            _actionHandlers[signal] = list;
        }
        list.Add(action);
    }

    // 시그널에 대응하는 액션 나열
    public IEnumerable<BaseAction> GetActionsFor(SignalType signal)
        => _actionHandlers.TryGetValue(signal, out var list) ? list : Array.Empty<BaseAction>();

    // Fire에서 버블 만들 때 사용할 실행 큐 생성
    public Queue<BaseAction> BuildActionQueue(SignalType signal)
        => new Queue<BaseAction>(GetActionsFor(signal));

    public bool HasActions(SignalType signal)
        => _actionHandlers.TryGetValue(signal, out var list) && list.Count > 0;

    public void SelfDestruct()
        => Owner?.RemoveProcessor(this);
}
