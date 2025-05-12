
// Processor.cs
using System;
using System.Collections.Generic;
using System.Linq;

public class Processor
{
    public string SourceName { get; }
    public bool IsBase { get; }
    public CardInstance Owner { get; }
    public CardInstance Source { get; }
    public int idx = 0;

    private readonly List<SignalHandler> handlers = new List<SignalHandler>();

    // 핸들러 재개 토큰
    public Token _handlerToken;

    public Processor(string sourceName, bool isBase, CardInstance owner, CardInstance source = null)
    {
        SourceName = sourceName;
        IsBase = isBase;
        Owner = owner;
        Source = source ?? owner;
    }

    public void Register(SignalType signal, Func<object, object> func)
    {
        handlers.Add(new SignalHandler(signal, func));
    }

    public IEnumerable<SignalHandler> GetHandlersFor(SignalType signal)
        => handlers.Where(h => h.Signal == signal);

    public void SelfDestruct() => Owner.RemoveProcessor(this);
    public bool HasRegistration(SignalType signal)
        => handlers.Any(h => h.Signal == signal);

    // 핸들러 토큰 등록
    public void UploadHandlerToken(object source, Action callback)
    {
        // 이미 콜백이 남아 있으면(=토큰 발행된 상태) 대기
        if (!_handlerToken.SourceEquals(this) && idx!=0)
        {
            GameManager.Instance._logs += " 핸들러재발행 ";
            return;
        }
        GameManager.Instance._logs += " 핸들러발행 ";
        _handlerToken = new Token(source, callback);
    }
    public Token UpdateHandlerToken(object source)
    {
        GameManager.Instance._logs += " 핸들러업데이트 ";
        _handlerToken.UpdateToken(source);
        return _handlerToken;
    }

    // 핸들러 토큰 실행
    public void ConsumeHandlerToken(object source)
    {
        GameManager.Instance._logs += " 토큰 처리 ";
        _handlerToken.InvokeIfSource(source);
    }

    /// <summary>
    /// 핸들러를 순차 실행하고, 모든 완료 후 저장된 매니저 토큰을 실행합니다.
    /// </summary>
    public void ProcessSignal(SignalType signal)
    {
        var list = GetHandlersFor(signal).ToList();

        if (list.Count == 0)
        {
            // 핸들러 없으면 바로 매니저 토큰 실행
            ReactionStackManager.Instance.ConsumeManagerToken(this);
            return;
        }
        ReactionStackManager.Instance.UpdateManagerToken(this);

        // 첫 핸들러 실행을 위한 토큰 준비
        UploadHandlerToken(this, () => InvokeHandler(list));

        ConsumeHandlerToken(this);
    }

    private void InvokeHandler(List<SignalHandler> list)
    {
        if (idx >= list.Count)
        {
            // 모든 핸들러 완료 후 매니저 토큰 실행
            ReactionStackManager.Instance.ConsumeManagerToken(this);
            return;
        }
        // 현재 핸들러 가져오기
        var handler = list[idx];

        // 다음 인덱스 예약
        idx++;

        // 다음 핸들러 토큰 예약
        UploadHandlerToken(this, () => InvokeHandler(list));

        // 동기 핸들러 실행
        handler.Process(this);

        // 이어서 다음
        ConsumeHandlerToken(this);
    }
}