using System.Collections.Generic;

public class ActionBubble
{
    private readonly Queue<BaseAction> _queue;
    public Processor OwnerProcessor { get; }
    public int PendingActions => _queue.Count;

    public ActionBubble(Queue<BaseAction> queue, Processor owner = null)
    {
        _queue = queue ?? new Queue<BaseAction>();
        OwnerProcessor = owner;
    }
    public ActionBubble(BaseAction action, Processor owner = null)
    {
        OwnerProcessor = owner;
        _queue = new Queue<BaseAction>();
        if (action != null) _queue.Enqueue(action);
    }

    // 버스가 한 번 호출하면, 토큰이 뺏기지 않는 한 스스로 계속 소비
    public void Next(SignalBus bus)
    {
        if (_queue.Count == 0)
        {
            bus.TrimFrontAndExpireIfEmpty();
            return;
        }
        GameManager.Instance._logs += $"Execute {_queue.Peek().GetType().Name} - ";
        _queue.Dequeue().Execute(bus);
    }
    public int GetPriority()
    {
        if (_queue.Count == 0)
            return int.MaxValue;

        // 큐 안에서 가장 먼저 나오는 ValueAction을 찾음
        foreach (var action in _queue)
        {
            if (action is ValueAction va)
                return va.priority;
        }

        // ValueAction이 전혀 없다면 → 우선순위 없음
        return int.MaxValue;
    }

}
