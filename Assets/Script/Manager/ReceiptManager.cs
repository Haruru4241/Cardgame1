using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class ReceiptManager : MonoBehaviour
{
    public static ReceiptManager Instance { get; private set; }
    private readonly List<ReceiptEntry> _entries = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Record(ReceiptEntry entry)
    {
        _entries.Add(entry);
    }
}
public class ReceiptEntry
{
    public int Turn;                  // 언제
    public SignalType Signal;         // 어떤 신호
    public BaseInstance Source;       // 발신자
    public List<BaseInstance> Targets;// 대상
    public string Result;             // 처리 결과 요약 ("파괴됨", "드로우 성공", "무효화" 등)
    public DateTime Timestamp;        // 시간 (디버깅용)
}
