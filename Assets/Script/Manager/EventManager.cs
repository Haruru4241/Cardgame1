using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ 사용
using System; // Func, Predicate 사용
using System.Text; // StringBuilder 사용

// --- 로그 유형 정의 (이전과 동일) ---
[Flags]
public enum LogType
{
    None = 0,
    BusProcessingStarted = 1 << 1,
    BusProcessingFinished = 1 << 2,
    ActionExecuting = 1 << 3,
    ActionExecuted = 1 << 4,
    CellAdded = 1 << 5,
    GameStateChanged = 1 << 6,
    CardDrawn = 1 << 7,
    CardPlayed = 1 << 8,
    DamageDealt = 1 << 9,
    ResourceChanged = 1 << 10,
    Draw = 1 << 11,
    Custom = 1 << 12,
    Shuffle = 1 << 13,
    Global = 1 << 14,
    Debug = 1 << 15,
    Target = 1 << 16,
Value=1<<17,
}

// --- 로그 항목 (버스 객체 참조) ---
[System.Serializable]
public class LogEntry
{
    public int LogId;
    public float Timestamp;
    public int TurnNumber;
    public LogType Type;
    public string Message;
    public BaseInstance Source;
    public List<BaseInstance> Targets;
    public SignalBus TriggeringBusContext;
    public SignalType Signal;

    // LogId 발급기 (EventManager에서 관리)
    // 생성자에서 triggeringBusId 대신 busContext 파라미터 받도록 수정
    internal LogEntry(int id, int turnNumber, LogType type, string message, SignalType signal, BaseInstance source = null, List<BaseInstance> targets = null, SignalBus busContext = null)
    {
        this.LogId = id;
        this.Timestamp = Time.time;
        this.TurnNumber = turnNumber;
        this.Type = type;
        this.Message = message;
        this.Source = source;
        this.Targets = targets;
        this.Signal = signal;
        this.TriggeringBusContext = busContext; // 버스 객체 참조 저장
    }
}


/// <summary>
/// 게임 내 발생하는 모든 상세 이벤트를 LogEntry 형태로 '독립적으로' 기록하고 관리하는 매니저입니다.
/// (SessionId 없이, 버스 객체 참조 사용)
/// </summary>
public class EventManager : MonoBehaviour
{
    // --- 싱글턴 설정 ---
    public static EventManager Instance { get; private set; }
    [SerializeField] private LogType _currentLogMask;

    // --- 로그 저장소 ---
    [SerializeField] private List<LogEntry> _allLogs = new List<LogEntry>();

    // --- 캐싱 (이번 턴 로그) ---
    private readonly List<LogEntry> _logsThisTurn = new List<LogEntry>();
    private int _currentTurnForCache = -1;

    // --- ID 발급기 ---
    private int _nextLogId = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 새로운 로그 항목을 생성하고 기록합니다. (외부 호출용)
    /// </summary>
    /// <param name="type">로그의 종류</param>
    /// <param name="message">로그 메시지</param>
    /// <param name="source">관련 인스턴스 (선택적)</param>
    /// <param name="busContext">이 로그를 발생시킨 현재 SignalBus 컨텍스트 (선택적, 연관성 추론용)</param>
    public void LogEvent(LogType type, string message, SignalType signal, BaseInstance source = null, List<BaseInstance> targets = null, SignalBus busContext = null)
    {
        int currentTurn = TurnManager.Instance.CurrentTurn;

        // 캐시 업데이트
        if (currentTurn != _currentTurnForCache)
        {
            _logsThisTurn.Clear();
            _currentTurnForCache = currentTurn;
        }

        // 새 로그 ID 발급 및 LogEntry 생성 (생성자에 busContext 전달)
        int newLogId = _nextLogId++;
        var newLog = new LogEntry(newLogId, currentTurn, type, message, signal, source, targets, busContext);
        _allLogs.Add(newLog);
        _logsThisTurn.Add(newLog);
    }


    // --- 검색 기능 (변경 없음) ---
    public List<LogEntry> FindLogs(Predicate<LogEntry> predicate)
    {
        return _allLogs.FindAll(predicate);
    }

    public List<LogEntry> FindLogsThisTurn(Predicate<LogEntry> predicate)
    {
        if (TurnManager.Instance.CurrentTurn == _currentTurnForCache)
        {
            return _logsThisTurn.FindAll(predicate);
        }
        else
        {
            return FindLogs(log => log.TurnNumber == TurnManager.Instance.CurrentTurn && predicate(log));
        }
    }

    // SessionId 관련 메서드 제거

    /// <summary>
    /// 모든 로그 기록을 삭제합니다.
    /// </summary>
    public void ClearLogs()
    {
        _allLogs.Clear();
        _logsThisTurn.Clear();
        _nextLogId = 0;
        _currentTurnForCache = -1;
    }
    /// <summary>
    // (Debug) 현재까지 기록된 모든 로그의 메시지를 Unity 콘솔에 출력합니다. (간략 버전)
    /// </summary>
    [ContextMenu("▶ (Debug) 모든 로그 메시지 출력 (간략)")]
    public void PrintAllLogMessagesToConsole()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"--- [EventLogger] 전체 로그 메시지 {_allLogs.Count}개 ---");

        if (_allLogs.Count == 0)
        {
            sb.Append("기록된 로그가 없습니다.");
        }
        else
        {
            var filteredLogs = _allLogs
            .Where(log => (_currentLogMask & log.Type) == 0) // 조건이 != 0 이 아닌 == 0 (포함되지 않은 것만 선택)
            .ToList();
            foreach (var log in filteredLogs)
            {
                // [T1] [OnCardPlayed] "공격 카드를 사용했습니다." 형식으로 출력
                sb.AppendLine($"[T{log.TurnNumber}] [{log.Type}] \"{log.Message}\"");
            }
        }

        // 최종 문자열을 콘솔에 한 번만 출력합니다.
        Debug.Log(sb.ToString());
    }
    // ──────────────────────────────────────────────────────────────────
    // [!! 수정된 !!] 계층형 로그 출력 (아이디어 1: Parent Bus 체이닝)
    // ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// (Debug) 현재까지 기록된 모든 로그를 '부모-자식' 버스 관계에 따라
    /// 계층형으로 Unity 콘솔에 출력합니다.
    /// </summary>
    [ContextMenu("▶ (Debug) 계층형 로그 콘솔에 출력 (Parent Bus)")]
    public void PrintLogHierarchyToConsole()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("--- [EventLogger] 계층형 로그 뷰 (Parent Bus) ---");
        var filteredLogs = _allLogs
            .Where(log => (_currentLogMask & log.Type) != 0) // 조건이 != 0 이 아닌 == 0 (포함되지 않은 것만 선택)
            .ToList();

        if (_allLogs.Count == 0)
        {
            sb.Append("기록된 로그가 없습니다.");
            Debug.Log(sb.ToString());
            return;
        }

        // --- 1. 로그를 버스(SignalBus) 단위로 그룹화 ---
        // (키 'bus'는 null이 아님이 보장됨)
        var logsByBus = new Dictionary<SignalBus, List<LogEntry>>();

        // --- 2. 버스 간의 부모-자식 관계 트리 생성 ---
        // [수정] Key: 부모 Bus (절대 null이 아님)
        // [수정] Value: 자식 Bus 리스트
        var busTree = new Dictionary<SignalBus, List<SignalBus>>();

        // [수정] 'null'을 키로 사용하는 대신, 최상위 버스(Root)를 별도 리스트로 관리
        var rootBuses = new List<SignalBus>();

        // --- 3. 모든 로그를 순회하며 'logsByBus', 'busTree', 'rootBuses'를 채웁니다 ---
        foreach (var log in _allLogs)
        {
            var bus = log.TriggeringBusContext;
            if (bus == null) continue; // 버스 컨텍스트가 없는 로그는 트리에 표시 불가

            // 3a. 'logsByBus' 딕셔너리에 로그 추가 (키 'bus'는 null이 아님)
            if (!logsByBus.ContainsKey(bus))
            {
                logsByBus[bus] = new List<LogEntry>();
            }
            // 로그가 여러 번 기록될 수 있으므로, 동일 로그 중복 추가 방지
            if (!logsByBus[bus].Contains(log))
            {
                logsByBus[bus].Add(log);
            }

            // 3b. 'busTree' 또는 'rootBuses'에 관계 추가 (버스당 1회만)
            var parentBus = bus.ParentBus;

            if (parentBus == null)
            {
                // [수정] 부모가 null이면 'rootBuses' 리스트에 추가
                if (!rootBuses.Contains(bus))
                {
                    rootBuses.Add(bus);
                }
            }
            else
            {
                // [수정] 부모가 null이 아니면 'busTree' 딕셔너리에 추가
                // (키 'parentBus'는 null이 아님)
                if (!busTree.ContainsKey(parentBus))
                {
                    busTree[parentBus] = new List<SignalBus>();
                }

                if (!busTree[parentBus].Contains(bus))
                {
                    busTree[parentBus].Add(bus);
                }
            }
        }

        // 4. 루트(Root) 버스부터 시작하여 재귀적으로 출력
        // 시간 순서대로 정렬하여 출력
        var sortedRootBuses = rootBuses
            .Where(bus => logsByBus.ContainsKey(bus)) // 로그가 있는 버스만
            .OrderBy(bus => logsByBus[bus].First().Timestamp)
            .ToList();

        foreach (var rootBus in sortedRootBuses)
        {
            PrintBusNode(rootBus, 0, sb, logsByBus, busTree);
        }

        Debug.Log(sb.ToString());
    }

    /// <summary>
    /// (재귀 헬퍼) 특정 버스 노드와 그에 속한 로그, 그리고 자식 버스들을 출력합니다.
    /// </summary>
    private void PrintBusNode(SignalBus bus, int depth, StringBuilder sb,
                              Dictionary<SignalBus, List<LogEntry>> logsByBus,
                              Dictionary<SignalBus, List<SignalBus>> busTree)
    {

        // 방어 코드: 버스 정보가 없으면 출력하지 않음
        if (bus == null || !logsByBus.ContainsKey(bus)) return;

        // 1. 이 버스의 대표 정보 출력
        string indent = new string(' ', depth * 4);
        var firstLog = logsByBus[bus].First(); // 버스의 대표 정보 (첫 번째 로그 기준)
        string sourceName = firstLog.Source?._data?.Name ?? "System";

        // 예: ▼ [OnCardPlayed] (from: 공격카드, logs: 2, depth: 0)
        sb.AppendLine($"{indent}▼ [{bus.Signal}] (from: {sourceName}, logs: {logsByBus[bus].Count}, depth: {bus.Depth})");

        // 2. 이 버스에 직접 속한 로그 메시지들 출력 (한 단계 더 들여쓰기)
        string childIndent = indent + "    ";
        foreach (var log in logsByBus[bus])
        {
            // 예:     ↳ [#001] 버스 처리 시작: OnCardPlayed
            // 예:     ↳ [#002] 티켓 만료 (버블 1개 처리 완료)
            sb.AppendLine($"{childIndent}↳ [#{log.LogId:D3}] {log.Message}");
        }

        // 3. 이 버스의 자식 버스들 재귀 호출 (busTree는 null 키를 가지지 않음)
        if (busTree.ContainsKey(bus))
        {
            // 자식 버스들도 시간순으로 정렬
            var childBuses = busTree[bus]
                .Where(b => logsByBus.ContainsKey(b)) // 로그가 있는 버스만
                .OrderBy(b => logsByBus[b].First().Timestamp)
                .ToList();

            foreach (var childBus in childBuses)
            {
                PrintBusNode(childBus, depth + 1, sb, logsByBus, busTree);
            }
        }
    }
}