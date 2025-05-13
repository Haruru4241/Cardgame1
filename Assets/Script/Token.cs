// Token.cs
using System;

/// <summary>
/// 콜백과 발행 주체(Source)를 함께 보관합니다.
/// </summary>
public struct Token
{
    public object Owner;    // 토큰 소유 주체
    public Action Callback;  // 실행할 콜백

    public Token(object source, Action callback)
    {
        Owner = source;
        Callback = callback;
    }
    public void UpdateToken(object source)
    {
        Owner = source;
    }
    public bool IsOwner(object candidate) => ReferenceEquals(candidate, Owner);
    public bool SourceEquals(object source)
    {
        if (ReferenceEquals(Owner, source))
        {
            return true;
        }
        return false;
    }

    public void InvokeIfSource(object expectedSource)
    {
        // 참조 동일성으로 비교
        if (ReferenceEquals(Owner, expectedSource))
        {
            GameManager.Instance._logs += string.Format("토큰 처리 성공");
            Callback?.Invoke();
            Callback = null;  // 실행 후 콜백 제거
            return;
        }
        GameManager.Instance._logs += string.Format("토큰 처리 실패");
    }
}