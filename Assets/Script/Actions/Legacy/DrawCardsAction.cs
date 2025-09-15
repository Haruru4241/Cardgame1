using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CardGame/Actions/DrawCardsAction")]
public class DrawCardsAction : BaseAction
{
    public int        drawCount    = 3;
    public SignalType driverSignal = SignalType.OnDrawDriver;

    public override void Execute(SignalBus bus)
    {
        // 1) 드로우 전용 버스 생성 + 소스 정보 복사
        var drawBus = new SignalBus(driverSignal, bus);
        drawBus.SetSourceInfo(bus.GetSourceCard());

        // 2) Processor 없이 곧장 버블 구성 (드로우 1회짜리 버블 x drawCount)
        var bubbles = new List<ActionBubble>(drawCount);
        for (int i = 0; i < drawCount; i++)
        {
            var q = new Queue<BaseAction>();
            // 각 버블은 독립적인 DrawAction 인스턴스를 가진다 (상태 분리 안전)
            q.Enqueue(ScriptableObject.CreateInstance<DrawAction>());
            bubbles.Add(new ActionBubble(q));
        }

        // 3) 승객 태우고 스택에 Push
        drawBus.AddPassengers(bubbles);
        ReactionStackManager.Instance.PushBus(drawBus);
    }
}
