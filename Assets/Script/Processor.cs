
using System.Collections.Generic;
using System.Linq;
using System;
public class Processor
{
    public string SourceName { get; }
    public bool IsBase { get; }
    public CardInstance Owner { get; }
    public CardInstance Source { get; }

    private List<SignalHandler> handlers = new();
    public Token _handlerToken;

    public Processor(string sourceName, bool isBase, CardInstance owner, CardInstance source = null)
    {
        SourceName = sourceName;
        IsBase = isBase;
        Owner = owner;
        Source = source ?? owner;
        _handlerToken = new Token(this, null);
    }

    public void Register(SignalType signal, Func<object, object> func)
    {
        handlers.Add(new SignalHandler(signal, func));
    }

    public IEnumerable<SignalHandler> GetHandlersFor(SignalType signal)
    {
        foreach (var handler in handlers)
            if (handler.Signal == signal)
                yield return handler;
    }
    public void SelfDestruct()
    {
        Owner.RemoveProcessor(this);
    }// 등록된 핸들러 중 특정 신호를 처리하는 핸들러가 있는지 검사합니다.
    public bool HasRegistration(SignalType signal)
    {
        return handlers.Any(h => h.Signal == signal);
    }

    // 핸들러 토큰 등록
    public void UploadHandlerToken(object source, Action callback)
    {
        // 이미 콜백이 남아 있으면(=토큰 발행된 상태) 대기
        if (!_handlerToken.SourceEquals(source)&&_handlerToken.Callback!=null)
        {
            GameManager.Instance._logs += " 핸들러재발행 ";
            return;
        }
        GameManager.Instance._logs += " 핸들러발행 ";
        _handlerToken.Callback = callback;
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


    public void ProcessSignal(SignalType signal, Action onProcDone)
    {
        // 1) 이 신호에 달린 핸들러들 목록을 복사해서
        var handlerQueue = new Queue<Action<Action>>();
        foreach (var h in GetHandlersFor(signal))
        {
            var copy = h;
            handlerQueue.Enqueue(done =>
            {
                // 동기 처리 예시
                copy.Process(null);
                done();
            });
        }
        // 2) 첫 핸들러부터 순차 처리
        // UploadHandlerToken(this, () => ProcessNextHandler(handlerQueue, onProcDone));
        // ConsumeHandlerToken(this);

        ProcessNextHandler(handlerQueue, onProcDone);
    }

    void ProcessNextHandler(Queue<Action<Action>> q, Action onProcDone)
    {
        if (q.Count == 0)
        {
            onProcDone();
            return;
        }

        var work = q.Dequeue();
        work(() => ProcessNextHandler(q, onProcDone));
    }
}