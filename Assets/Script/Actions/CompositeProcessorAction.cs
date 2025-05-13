using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "CardGame/Actions/CompositeProcessorAction")]
public class CompositeProcessorAction : CardAction
{
    [System.Serializable]
    public class SignalActionEntry
    {
        public SignalType signal;
        public List<CardAction> actions;
    }

    public string processorName = "복합 효과";
    public List<SignalActionEntry> entries;

    // 자기 자신에게 실행
    public override void Execute(CardInstance card, Processor processor)
    {
        var proc = new Processor(processorName, false, card, card);

        // 핵심: GetFunction으로 등록 로직 수행
        var registerFunc = GetFunction(proc);
        registerFunc(null);

        card.AddProcessor(proc);
    }

    // 기존 프로세서에 레지스터만 수행
    public override Func<object, object> GetFunction(Processor self)
    {
        return input =>
        {
            foreach (var entry in entries)
                foreach (var action in entry.actions)
                    self.Register(entry.signal, action.GetFunction(self));

            return null;
        };
    }
}
