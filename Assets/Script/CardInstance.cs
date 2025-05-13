using System.Collections.Generic;
using System;                  // Action 델리게이트를 위해
using System.Linq;             // ToList() 확장 메서드를 위해
using UnityEngine;
public class CardInstance
{
    public BaseCard BaseCard { get; set; }
    public Pile CurrentPile { get; set; }

    public CardData CardData { get; private set; }
    public List<Processor> processors = new();

    public CardInstance(CardData data)
    {
        CardData = data;

        CurrentPile = null;


        // 이름
        AddProcessor(CreateBaseProcessor<string>(
            SignalType.NameEvaluation,
            () => data.cardName,
            "기본 이름"));

        // 설명
        AddProcessor(CreateBaseProcessor<string>(
            SignalType.DescriptionEvaluation,
            () => data.description,
            "기본 설명"));

        // 마나 코스트
        AddProcessor(CreateBaseProcessor<int>(
            SignalType.ManaCostEvaluation,
            () => data.manaCost,
            "기본 코스트"));

        AddProcessor(CreateBaseProcessor<int>(
            SignalType.BuyCostEvaluation,
            () => data.cost,
            "기본 코스트"));

        // 아트워크
        AddProcessor(CreateBaseProcessor<UnityEngine.Sprite>(
            SignalType.ArtworkEvaluation,
            () => data.artwork,
            "기본 아트워크"));

        RegisterProcessor(SignalType.OnDraw, data.onDrawActions);
        RegisterProcessor(SignalType.OnSelect, data.onSelectActions);
        RegisterProcessor(SignalType.OnUnSelect, data.onUnSelectActions);
        RegisterProcessor(SignalType.OnRequirement, data.onRequirementActions);
        RegisterProcessor(SignalType.OnUse, data.onUseActions);
        RegisterProcessor(SignalType.OnEffect, data.onEffectActions);
        RegisterProcessor(SignalType.OnPlayed, data.onPlayedActions);
        RegisterProcessor(SignalType.OnDiscard, data.onDiscardActions);
        RegisterProcessor(SignalType.OnExhaust, data.onExhaustActions);
        RegisterProcessor(SignalType.OnDestroy, data.onDestroyActions);
    }
    public void RegisterProcessor(SignalType signal, List<CardAction> actions)
    {
        if (actions == null || actions.Count == 0) return;

        var processor = new Processor($"CardData_{signal}", isBase: false, owner: this);

        foreach (var action in actions)
        {
            processor.Register(signal, _ =>
            {
                action.Execute(this, processor);
                return null;
            });
        }

        AddProcessor(processor);
    }
    public struct SignalProcessorBinding
    {
        public SignalType Signal;
        public string ProcessorName;
    }
    public List<SignalProcessorBinding> GetSignalProcessorBindings()
    {
        var bindings = new List<SignalProcessorBinding>();

        // for every processor on this card
        foreach (var proc in processors)
        {
            // reflect over all possible SignalType values
            foreach (SignalType sig in Enum.GetValues(typeof(SignalType)))
            {
                // if this processor has at least one handler for that signal
                if (proc.GetHandlersFor(sig).Any())
                {
                    bindings.Add(new SignalProcessorBinding
                    {
                        Signal = sig,
                        ProcessorName = proc.SourceName   // or whatever your Processor calls its name field
                    });
                }
            }
        }

        return bindings;
    }

    // 공통 Processor 등록 도우미
    private Processor CreateBaseProcessor<T>(SignalType signal, System.Func<T> getter, string sourceName)
    {
        var processor = new Processor(sourceName, isBase: true, owner: this);
        processor.Register(signal, _ => getter());
        return processor;
    }

    public void AddProcessor(Processor processor)
    {
        processors.Add(processor);
    }
    public void RemoveProcessor(Processor processor)
    {
        if (processors.Contains(processor))
        {
            processors.Remove(processor);
        }
    }

    public T Evaluate<T>(SignalType signal)
    {
        object result = default(T);

        // base 먼저 적용
        foreach (var proc in processors)
        {
            if (proc.IsBase)
            {
                foreach (var handler in proc.GetHandlersFor(signal))
                    result = handler.Process(default(T));
            }
        }

        // 그 외 변형 적용
        foreach (var proc in processors)
        {
            if (!proc.IsBase)
            {
                foreach (var handler in proc.GetHandlersFor(signal))
                    result = handler.Process(result);
            }
        }

        return (T)result;
    }

    public void Fire(SignalType signal)
    {
        // 1) 이 신호에 반응할 프로세서만 골라서 리스트 생성
        var reactingProcs = processors
            .Where(proc => proc.GetHandlersFor(signal).Any())
            .ToList();

        if (reactingProcs.Count == 0)
            return;
        GameManager.Instance._logs+=" "+signal+" 파이어 시작 ";

        // 2) 스택에 푸시
        ReactionStackManager.Instance.PushReactions(signal, reactingProcs);
    }

}