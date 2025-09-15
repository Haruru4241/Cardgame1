using System;
using UnityEngine;

public enum CellKind { None, Int, Float, String, Signal }
public enum CalcOp  { Set, Add, Sub, Mul, Div, Clamp }

public class Cell
{
    public CellKind Kind { get; private set; } = CellKind.None;
    public object   Value { get; private set; } = null;

    public override string ToString() => $"Cell<{Kind}>({Value})";

    public void Reset() { Kind = CellKind.None; Value = null; }

    public bool Apply(CalcOp op, object a, object b = null)
    {
        try
        {
            switch (op)
            {
                case CalcOp.Set:   return DoSet(a);
                case CalcOp.Add:   return DoAdd(a);
                case CalcOp.Sub:   return DoNum((x,y)=>x-y, a, "Sub");
                case CalcOp.Mul:   return DoNum((x,y)=>x*y, a, "Mul");
                case CalcOp.Div:   return DoNum((x,y)=>{ if (Math.Abs(y)<1e-12) throw new DivideByZeroException(); return x/y; }, a, "Div");
                case CalcOp.Clamp: return DoClamp(a, b);
                default:           Warn($"지원하지 않는 연산 {op}"); return false;
            }
        }
        catch (Exception e) { Warn($"연산 실패 [{op}]: {e.Message}"); return false; }
    }

    bool DoSet(object a)
    {
        if (!CoerceSetTargetKind(a, out var kind, out var boxed))
        { Warn($"Set 불가: 타입 {TypeName(a)}"); return false; }
        Kind = kind; Value = boxed; return true;
    }

    bool DoAdd(object a)
    {
        if (Kind == CellKind.None) return DoSet(a);
        if (Kind == CellKind.Signal) { Warn("Signal(enum)에는 + 금지"); return false; }
        if (Kind == CellKind.String) { Value = (string)Value + AsString(a); return true; }

        if (!TryAsNumber(Value, out var cur, out var curF)) { Warn("+ 불가: 현재 타입이 숫자 아님"); return false; }
        if (!TryAsNumber(a,     out var rhs, out var rhsF)) { Warn("+ 불가: 피연산자가 숫자 아님"); return false; }
        ApplyNumericResult(cur + rhs, curF || rhsF); return true;
    }

    bool DoNum(Func<double,double,double> f, object a, string opName)
    {
        if (Kind == CellKind.Signal || Kind == CellKind.String) { Warn($"{Kind}에는 {opName} 금지"); return false; }
        if (Kind == CellKind.None) { Kind = IsFloaty(a) ? CellKind.Float : CellKind.Int; Value = Kind==CellKind.Float ? (object)0f : 0; }

        if (!TryAsNumber(Value, out var cur, out var curF)) { Warn($"{opName} 불가: 현재 값 숫자 아님"); return false; }
        if (!TryAsNumber(a,     out var rhs, out var rhsF)) { Warn($"{opName} 불가: 피연산자 숫자 아님"); return false; }
        var res = f(cur, rhs);
        ApplyNumericResult(res, curF || rhsF || opName=="Div"); return true;
    }

    bool DoClamp(object min, object max)
    {
        if (Kind == CellKind.Signal || Kind == CellKind.String) { Warn($"{Kind}에는 Clamp 금지"); return false; }
        if (Kind == CellKind.None) { Kind = CellKind.Int; Value = 0; }
        if (!TryAsNumber(Value, out var v,  out var vf) ||
            !TryAsNumber(min,   out var mn, out var mf) ||
            !TryAsNumber(max,   out var mx, out var xf)) { Warn("Clamp 불가: 숫자 아님"); return false; }
        if (mn > mx) (mn, mx) = (mx, mn);
        ApplyNumericResult(Math.Min(Math.Max(v, mn), mx), vf||mf||xf); return true;
    }

    void ApplyNumericResult(double v, bool toFloat)
    {
        if (toFloat) { Kind = CellKind.Float; Value = (float)v; }
        else         { Kind = CellKind.Int;   Value = Mathf.RoundToInt((float)v); }
    }

    static bool CoerceSetTargetKind(object a, out CellKind kind, out object boxed)
    {
        if (a is null) { kind = CellKind.None; boxed = null; return true; }
        switch (a)
        {
            case int i:     kind=CellKind.Int;    boxed=i;        return true;
            case long l:    kind=CellKind.Int;    boxed=(int)l;   return true;
            case float f:   kind=CellKind.Float;  boxed=f;        return true;
            case double d:  kind=CellKind.Float;  boxed=(float)d; return true;
            case string s:  kind=CellKind.String; boxed=s;        return true;
            case SignalType sig: kind=CellKind.Signal; boxed=sig; return true;
            default:
                if (a.GetType().IsEnum) { kind=CellKind.Signal; boxed=a; return true; }
                kind=CellKind.None; boxed=null; return false;
        }
    }

    static bool TryAsNumber(object o, out double d, out bool isFloaty)
    {
        switch (o)
        {
            case int i:     d=i; isFloaty=false; return true;
            case long l:    d=l; isFloaty=false; return true;
            case float f:   d=f; isFloaty=true;  return true;
            case double db: d=db;isFloaty=true;  return true;
            default:        d=0; isFloaty=false; return false;
        }
    }

    static bool IsFloaty(object o) => o is float || o is double;
    static string AsString(object o) => o switch { null=>"null", SignalType s=>s.ToString(), _=>o.ToString() };
    static string TypeName(object o) => o?.GetType().Name ?? "null";

    static void Warn(string msg)
    {
        Debug.LogWarning($"[Cell] {msg}");
        if (GameManager.Instance != null) GameManager.Instance._logs += $" [Cell] {msg}\n";
    }
}
