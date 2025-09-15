// ───────────────────────────────────────────────────────────
// 파일명: SignalBus.cs (요구사항 반영)
// 1) _byProc 제거
// 2) AddPassengers: 외부(Fire)에서 만든 ActionBubble을 그대로 탑승
// 3) ProcessSignalAction(): while 제거, 맨 앞 버블 1회 Next만 지시
//    - 실행 전/후 맨 앞에서만 Removable 정리(순서 보존)
// 4) bus.CurrentBubble 사용 안 함
// 5) 매니저가 상태를 판단할 수 있도록 IsFinished/IsBlocked 제공
// ───────────────────────────────────────────────────────────
using System.Collections.Generic;
using System;
using System.Linq;
public class SignalBus
{
    public SignalType Signal { get; set; }
    public int Depth { get; set; }
    public SignalBus ParentBus { get; private set; }

    // Source 정보 유지
    public BaseInstance SourceObject;

    public bool HasToken { get; private set; } = true;

    // 외부(Fire)에서 생성한 버블들을 그대로 탑승
    public List<ActionBubble> _bubbles = new();
    public ActionBubble FrontBubble => _bubbles.Count > 0 ? _bubbles[0] : null;

    // --- 계산 셀: 버스당 1개 ---
    private readonly Cell _cell = new Cell();

    // 공개 접근자(원하면 private set 등으로 조절)
    public CellKind CalcKind => _cell.Kind;
    public object CalcRaw => _cell.Value;

    public SignalBus(SignalType signal, SignalBus parentBus = null)
    {
        Signal = signal;
        ParentBus = parentBus;
        Depth = (parentBus != null) ? parentBus.Depth + 1 : 0;
    }

    // ── 승객 탑승(복수만 제공; 단일은 new[]{bubble}로 호출)
    public void AddPassengers(IEnumerable<ActionBubble> bubbles)
    {
        if (bubbles == null) return;
        foreach (var b in bubbles)
            if (b != null) _bubbles.Add(b);
    }
    public void AddPassenger(ActionBubble bubble)
    {
        if (bubble != null) _bubbles.Add(bubble);
    }

    // ── 실행: 맨 앞 버블에만 1회 Next 지시 (버블 내부 재귀 소비)
    public void ProcessSignalAction()
    {
        if (_bubbles.Count == 0 || !HasToken) return;

        GameManager.Instance._logs += "버스 시작 ";
        _bubbles[0].Next(this);
        ProcessSignalAction();
    }
    public void TrimFrontAndExpireIfEmpty()
    {
        // 실행 전/후 공통 사용 가능
        _bubbles.RemoveAt(0);
        ReactionStackManager.Instance.Continue();
    }
    public bool TryTakeToken()
    {
        if (!HasToken) return false;
        HasToken = false;
        return true;
    }
    public void ReturnToken() => HasToken = true;

    // ── 상태/정보 (매니저가 판단용으로 조회)
    public bool IsFinished => _bubbles.Count == 0;
    public bool HasPassenger() => _bubbles.Count > 0;
    public int PassengerCount => _bubbles.Count;

    // ── Source/Payload
    public void SetSourceInfo(BaseInstance card)
    {
        SourceObject = card;
    }
    public void SortBubblesByPriority()
    {
        if (_bubbles == null || _bubbles.Count <= 1)
            return;

        // 안정 정렬을 위해 OrderBy 사용
        _bubbles = _bubbles
            .OrderBy(b => b.GetPriority())
            .ToList();
    }


    public BaseInstance GetSourceCard() => SourceObject;

    // SignalBus 내부
    public bool Calc(CalcOp op, object a, object b = null) => _cell.Apply(op, a, b);
    public void CalcReset() => _cell.Reset(); // 선택
}
