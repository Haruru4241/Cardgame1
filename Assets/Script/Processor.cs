using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Processor
{
    public string SourceName { get; }
    public bool IsBase { get; }
    public CardInstance Owner { get; }
    public CardInstance Source { get; }

    private List<SignalHandler> handlers = new();
    public Token _handlerToken;

    // 새로 추가된 필드들
    private Queue<SignalHandler> _handlerQueue;

    public Processor(string sourceName, bool isBase, CardInstance owner, CardInstance source = null)
    {
        SourceName = sourceName;
        IsBase = isBase;
        Owner = owner;
        Source = source ?? owner;
        _handlerToken = new Token(this, null);
        //_handlerQueue = new Queue<SignalHandler>();
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
    }

    public void UploadHandlerToken(object source, Action callback)
    {
        if (!_handlerToken.SourceEquals(source))
        {
            GameManager.Instance._logs += " 잘못된 소유주 ";
            return;
        }
        GameManager.Instance._logs += " 핸들러발행 ";
        _handlerToken.Callback = callback;
    }

    public Token UpdateHandlerToken(object source)
    {
        _handlerToken.UpdateToken(source);
        return _handlerToken;
    }

    public void ConsumeHandlerToken(object source)
    {
        _handlerToken.InvokeIfSource(source);
    }

    public void ProcessSignal(SignalType signal)
    {
        // 아직 큐가 없으면 새로 만들기
        //if (_handlerQueue==null&&_handlerQueue.Count == 0)
        if (_handlerQueue==null)
        {
            _handlerQueue = new Queue<SignalHandler>(GetHandlersFor(signal));
            GameManager.Instance._logs += string.Format(" 프로세서 큐 생성 {0}개 ", _handlerQueue.Count);
        }
        GameManager.Instance._logs += string.Format(" {0} 신호 받음 ", signal);

        // 토큰 등록 → 처리 재개
        UploadHandlerToken(this, () => ProcessNextHandler());
        ConsumeHandlerToken(this);
    }

    private void ProcessNextHandler()
    {
        if (_handlerQueue.Count == 0)
        {
            GameManager.Instance._logs += string.Format(" 프로세서 종료 ");
            
            _handlerQueue=null;
            ReactionStackManager.Instance._queue.RemoveAt(0);
            ReactionStackManager.Instance.ProcessNext();
            return;
        }

        // 다음 SignalHandler 꺼내서 동기 실행
        UploadHandlerToken(this, () => ProcessNextHandler());
        _handlerQueue.Dequeue().Process(this);
        ConsumeHandlerToken(this);
    }
}
