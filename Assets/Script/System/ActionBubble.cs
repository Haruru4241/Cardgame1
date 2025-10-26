using System.Collections.Generic;

public class ActionBubble
{
    public Queue<BaseAction> _queue;
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
        // if (_queue.Count == 0)
        // {
        //     bus.TrimFrontAndExpireIfEmpty();
        //     return;
        // }
        //GameManager.Instance._logs += $"Execute {_queue.Peek().GetType().Name} - ";
        EventManager.Instance.LogEvent(LogType.ActionExecuted, $"Execute {_queue.Peek().GetType().Name}", bus.Signal, null, null, bus);
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
    public Queue<BaseAction> GetActions(){ return _queue; }

    public ActionBubble Clone()
    {
        // 큐의 BaseAction 레퍼런스들을 복사하여 새 큐를 만듭니다.
        var newActionQueue = new Queue<BaseAction>(this._queue);
        var newBubble = new ActionBubble(newActionQueue, this.OwnerProcessor);
        return newBubble;
    }
}
