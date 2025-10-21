using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ 사용
using System; // Func, Predicate 사용

// --- 로그 유형 정의 (이전과 동일) ---
public enum LogType
{
    BusProcessingStarted,
    BusProcessingFinished,
    ActionExecuting,
    ActionExecuted,
    CellAdded,
    GameStateChanged,
    CardDrawn,
    CardPlayed,
    DamageDealt,
    ResourceChanged,
    Custom
}

// --- 로그 항목 (버스 객체 참조) ---
[System.Serializable]
public class LogEntry
{
    public int LogId { get; }
    public float Timestamp { get; }
    public int TurnNumber { get; }
    public LogType Type { get; }
    public string Message { get; }
    public BaseInstance Source { get; }
    public <List>BaseInstance Targets { get; }
    public SignalBus TriggeringBusContext { get; }

    // LogId 발급기 (EventManager에서 관리)
    // 생성자에서 triggeringBusId 대신 busContext 파라미터 받도록 수정
    internal LogEntry(int id, int turnNumber, LogType type, string message, BaseInstance source = null, SignalBus busContext = null)
    {
        this.LogId = id;
        this.Timestamp = Time.time;
        this.TurnNumber = turnNumber;
        this.Type = type;
        this.Message = message;
        this.Source = source;
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

    // --- 로그 저장소 ---
    private readonly List<LogEntry> _allLogs = new List<LogEntry>();

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
    public void LogEvent(LogType type, string message, BaseInstance source = null, SignalBus busContext = null)
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
        var newLog = new LogEntry(newLogId, currentTurn, type, message, source, busContext);
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
}