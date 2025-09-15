// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// public enum LogType
// {
//     System,
//     Draw,
//     Selection,
//     Resource,
//     Processor,
//     Warning,
//     Error
// }

// public class LogMessage
// {
//     public LogType Type;
//     public string Message;
//     public DateTime Time;
//     public object Source;

//     public LogMessage(LogType type, string message, object source = null)
//     {
//         Type = type;
//         Message = message;
//         Time = DateTime.Now;
//         Source = source;
//     }

//     public override string ToString()
//     {
//         return $"[{Time:HH:mm:ss}][{Type}] {Message}";
//     }
// }

// public class LogSystem
// {
//     public static LogSystem Instance { get; } = new LogSystem();
//     private List<LogMessage> logs = new();

//     public void Log(LogType type, string message, object source = null)
//     {
//         var log = new LogMessage(type, message, source);
//         logs.Add(log);
//         Debug.Log(log.ToString());
//     }

//     public void LogBus(SignalBus bus, string note = "")
//     {
//         string chainInfo = TraceBusChain(bus);
//         string payloadInfo = string.Join(", ", bus.Payload.Select(kv => $"{kv.Key}={kv.Value}"));
//         string msg = $"[{bus.Signal}] {note} / from: {bus.SourceObject?.ToString() ?? "null"} / Payload: {payloadInfo} / Chain: {chainInfo}";

//         Log(LogType.System, msg, bus.SourceObject);
//     }

//     private string TraceBusChain(SignalBus bus)
//     {
//         List<string> chain = new();
//         var current = bus;

//         while (current != null)
//         {
//             chain.Add(current.Signal.ToString());
//             current = current.SourceBus;
//         }

//         chain.Reverse();
//         return string.Join(" → ", chain);
//     }

//     public IEnumerable<LogMessage> GetLogs(LogType? filter = null)
//     {
//         return filter == null ? logs : logs.Where(l => l.Type == filter);
//     }

//     public IEnumerable<LogMessage> GetLogsFrom(object source)
//     {
//         return logs.Where(l => l.Source == source);
//     }

//     public IEnumerable<LogMessage> GetLogsByKeyword(string keyword)
//     {
//         return logs.Where(l => l.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase));
//     }

//     public void Clear() => logs.Clear();
// }
