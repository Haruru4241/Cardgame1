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
    private Token _handlerToken;

    // 새로 추가된 필드들
    private Queue<SignalHandler> _handlerQueue;

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

    public void ProcessSignal(SignalBus bus)
    {
        // 1) 아직 큐가 없으면, 버스에 해당하는 핸들러들을 모아서 새로 생성
        if (_handlerQueue == null)
        {
            // 버스 내부에 담긴 SignalType을 사용
            _handlerQueue = new Queue<SignalHandler>(GetHandlersFor(bus.Signal));
            GameManager.Instance._logs += string.Format(" 프로세서 큐 생성 {0}개 ", _handlerQueue.Count);
        }
        GameManager.Instance._logs += string.Format(" {0} 신호 받음 ", bus.Signal);

        // 토큰 등록 → 처리 재개
        UploadHandlerToken(this, () => ProcessNextHandler(1));
        ConsumeHandlerToken(this);
    }

    private void ProcessNextHandler(int a)
    {
        if (_handlerQueue.Count == 0)
        {
            GameManager.Instance._logs += string.Format(" 프로세서 종료 ");

            _handlerQueue = null;
            var bus = ReactionStackManager.Instance._busStack.Pop();
            bus.DequeueNext();

            ReactionStackManager.Instance.StartProcessing();
            return;
        }

        // 다음 SignalHandler 꺼내서 동기 실행
        UploadHandlerToken(this, () => ProcessNextHandler(1));
        _handlerQueue.Dequeue().Process(null);
        ConsumeHandlerToken(this);
    }
}
