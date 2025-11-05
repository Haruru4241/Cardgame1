using System.Collections.Generic;
using System;                  // Action 델리게이트를 위해
using System.Linq;             // ToList() 확장 메서드를 위해
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems; // [!] 드래그 인터페이스 사용

public abstract class BaseInstance
{
    public List<Processor> _processors = new();
    public BaseController controller { get; set; }
    public Zone CurrentZone { get; set; }
    public BaseInstance Owner { get; set; }

    public BaseData _data { get; protected set; }

    public void AddProcessor(Processor p)
    {
        // 핵심 로직을 재사용합니다.
        AddProcessors(new[] { p });
    }
    public void AddProcessors(IEnumerable<Processor> processorsToAdd)
    {
        foreach (var p in processorsToAdd)
        {
            if (p != null)
            {
                _processors.Add(p);
            }
        }

        controller?.UpdateUI();
    }
    public void RemoveProcessor(Processor processor)
    {
        // 핵심 로직을 재사용합니다.
        RemoveProcessors(new[] { processor });
    }
    public void RemoveProcessors(IEnumerable<Processor> processorsToRemove)
    {
        foreach (var p in processorsToRemove)
        {
            if (_processors.Contains(p))
            {
                _processors.Remove(p);
                controller?.UpdateUI();
            }
        }

        controller?.UpdateUI();
    }
    public void RegisterProcessor(SignalType signal, List<BaseAction> actions)
    {
        if (actions == null || actions.Count == 0) return;

        var processor = new Processor($"CardData_{signal}", isBase: false, owner: this, source: this);

        foreach (var action in actions)
        {
            processor.RegisterAction(signal, action);
        }

        AddProcessor(processor);
    }
    public IEnumerable<Processor> GetProcessorsFor(SignalBus bus)
    {
        return _processors.Where(p => p.GetActionsFor(bus.Signal).Any());
    }

    // 2) CreateBaseProcessor → getter 함수만 넘기면 SO를 만들어 등록
    protected Processor CreateBaseProcessorAction(
    SignalType signal,
    object value,           // int/float/string/SignalType 모두 가능
    CalcType typeToProcess,
    CalcOp op = CalcOp.Set  // Set / Add / Sub
)
    {
        var action = ScriptableObject.CreateInstance<ValueAction>();
        action.Initialize(op, value, typeToProcess);

        var proc = new Processor($"Base_{signal}", isBase: true, owner: this, source: this);
        proc.RegisterAction(signal, action);
        return proc;
    }
    public List<ActionBubble> BuildBubblesForSignal(SignalBus bus)
    {
        var reactingProcs = GetProcessorsFor(bus).ToList();
        var bubbles = new List<ActionBubble>(reactingProcs.Count);

        foreach (var p in reactingProcs)
        {
            var q = p.BuildActionQueue(bus.Signal);
            if (q != null && q.Count > 0)
                bubbles.Add(new ActionBubble(q, p));
        }

        return bubbles;
    }
    public SignalBus PrepareBus(SignalBus bus)
    {
        var Bubbles = BuildBubblesForSignal(bus);
        if (Bubbles.Count == 0) return bus; // 실행할 액션이 없으면 null 반환

        bus.AddPassengers(Bubbles);
        bus.SetSourceInfo(this);

        return bus;
    }/// <summary>
     /// 현재 카드의 상태를 바탕으로, 설명문 템플릿 안의 모든 동적 토큰({ID})을
     /// 실제 계산된 값으로 교체하여 최종 문자열을 반환합니다.
     /// </summary>
    public string GetUpdatedDescription()
    {
        // 1. 먼저, 이 인스턴스의 '설명문 템플릿'을 계산해서 가져옵니다.
        var descBus = new SignalBus(SignalType.DescriptionEvaluation);
        this.PrepareBus(descBus);
        ReactionStackManager.Instance.PushBus(descBus);
        string template = CalculationManager.Instance.Evaluate<string>(descBus, CalcType.Description);

        if (string.IsNullOrEmpty(template)) return "";

        // 2. 템플릿 안의 {ID}를 찾아 실제 값으로 교체합니다.
        return Regex.Replace(template, @"\{(.+?)\}", match =>
        {
            string tokenID = match.Groups[1].Value;
            DescriptionEntry foundEntry = null;
            object baseValue = null;

            // 자신의 데이터(_data)에 있는 액션 목록을 탐색하여 ID에 맞는 액션을 찾습니다.
            foreach (var actionList in this._data.actionEntries)
            {
                foreach (var action in actionList.actions)
                {
                    baseValue = action.GetValueForTokenID(tokenID, null);
                    if (baseValue != null)
                    {
                        foundEntry = action.descriptionEntries.Find(e => e.TokenID == tokenID);
                        if (foundEntry != null) break;
                    }
                }
                if (foundEntry != null) break;
            }

            // ID에 맞는 액션과 엔트리를 모두 찾았다면, 실제 값 계산을 수행합니다.
            if (baseValue != null && foundEntry != null)
            {
                // '미리보기 버스'를 생성합니다.
                var valueBus = new SignalBus(foundEntry.EvaluationSignal);

                // ContextResolverManager를 통해 현재 상황에 맞는 모든 관련자를 수집합니다.
                List<BaseInstance> relevantInstances = ContextResolverManager.Instance.GetRelevantInstances(this);

                // 수집된 관련자들의 Processor를 버스에 추가합니다.
                foreach (var instance in relevantInstances)
                {
                    instance.PrepareBus(valueBus);
                }

                // 기본값을 Set 연산으로 버스에 추가하고 최종 값을 계산합니다.
                valueBus.AddCalculationStep(new Cell(foundEntry.ValueType, CalcOp.Set, baseValue));
                ReactionStackManager.Instance.PushBus(valueBus);

                object finalValue = CalculationManager.Instance.Evaluate<object>(valueBus, foundEntry.ValueType);

                return finalValue.ToString();
            }

            return match.Value; // 일치하는 ID가 없으면 {ID} 그대로 반환
        });
    }

    public abstract void Fire(SignalBus bus);
}
