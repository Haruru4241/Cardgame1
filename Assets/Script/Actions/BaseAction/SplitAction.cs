using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Actions/SplitAction")]
public class SplitAction : BaseAction
{
    public BaseAction Action;

    public override void Execute(SignalBus bus)
    {
        if (bus == null || bus.CalcKind != CellKind.Int)return;

        // 1) 셀에서 드로우 개수 해석
        int count = Mathf.Max(0, (int)bus.CalcRaw);

        // 2) 드로우 전용 버스 생성 + 소스 정보 복사
        var drawBus = new SignalBus(bus.Signal, bus);
        drawBus.SetSourceInfo(bus.GetSourceCard());

        // 3) Processor 없이 곧장 버블 구성 (드로우 1회짜리 버블 x count)
        var bubbles = new List<ActionBubble>(count);
        for (int i = 0; i < count; i++)
        {
            var q = new Queue<BaseAction>();
            q.Enqueue(Action); // 상태 분리를 위해 인스턴스 분리
            bubbles.Add(new ActionBubble(q));
        }

        // 4) 승객 태우고 스택에 Push
        drawBus.AddPassengers(bubbles);
        GameManager.Instance._logs += $"드로우: {count} ";
        ReactionStackManager.Instance.PushBus(drawBus);
    }
}
