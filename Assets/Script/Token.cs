// Token.cs
using System;

/// <summary>
/// 콜백과 발행 주체(Source)를 함께 보관합니다.
/// </summary>
public struct Token
{
    public object Source;    // 토큰 발행 주체 (Processor 인스턴스 등)
    public Action Callback;  // 실행할 콜백

    public Token(object source, Action callback)
    {
        Source = source;
        Callback = callback;
    }
    public void UpdateToken(object source)
    {
        Source = source;
    }
    public bool SourceEquals(object source)
    {
        if (ReferenceEquals(Source, source))
        {
            return true;
        }
        return false;
    }

    public void InvokeIfSource(object expectedSource)
    {
        // 참조 동일성으로 비교
        if (ReferenceEquals(Source, expectedSource))
        {
            Callback?.Invoke();
            Callback = null;  // 실행 후 콜백 제거
        }
    }
}