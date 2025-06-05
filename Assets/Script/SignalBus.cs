// ───────────────────────────────────────────────────────────
// 파일명: SignalBus.cs
// 주요 변경: 승객을 추가하는 AddPassengers 메서드 추가
// ───────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;

public class SignalBus
{
    /// <summary>
    /// 이 버스가 처리할 시그널 종류
    /// </summary>
    public SignalType Signal { get; }
    public int CurrentDepth { get; set; }

    // 새로 추가: 부모 버스(이 버스를 생성한 출처)
    public SignalBus ParentBus { get; private set; }

    /// <summary>
    /// 이 버스에 탑승한(반응해야 할) Processor들의 대기열
    /// </summary>
    private Queue<Processor> _passengers;

    /// <summary>
    /// 생성자: 시그널 타입만 넘겨받고, 승객 큐는 일단 비워둡니다.
    /// </summary>
    public SignalBus(SignalType signal)
    {
        Signal = signal;

        CurrentDepth = 0;
        _passengers = new Queue<Processor>();
    }
    public SignalBus(SignalType driverSignal, SignalBus parent) : this(driverSignal)
    {
        ParentBus = parent;
    }
    

    /// <summary>
    /// 외부에서 승객(Processor)들의 리스트를 한꺼번에 받아서 내부 큐에 Enqueue
    /// </summary>
    public void AddPassengers(List<Processor> procs)
    {
        if (procs == null || procs.Count == 0)
            return;

        foreach (var p in procs)
            _passengers.Enqueue(p);
    }

    /// <summary>
    /// 남은 탑승객(Processor)을 하나 꺼내 반환.
    /// 더 이상 없으면 null 반환.
    /// </summary>
    public Processor DequeueNext()
    {
        if (_passengers.Count == 0)
            return null;
        return _passengers.Dequeue();
    }
    public Processor PeekNextPassenger()
    {
        return _passengers.Count > 0 ? _passengers.Peek() : null;
    }

    /// <summary>
    /// 승객이 하나라도 남아 있는지 체크
    /// </summary>
    public bool HasPassenger()
    {
        return _passengers.Count > 0;
    }
    public int PassengerCount
    {
        get { return _passengers.Count; }
    }
}
