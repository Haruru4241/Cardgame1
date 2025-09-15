// using UnityEngine;

// public class CostReducerAction : BaseAction
// {
//     [Tooltip("더해질(혹은 뺄) 값. 코스트 감소는 음수(-1 등)로 설정")]
//     public int delta = -1;

//     [Tooltip("이 감소가 반영될 평가 신호 (예: ManaCostEvaluation)")]
//     public SignalType signal = SignalType.ManaCostEvaluation;

//     /// <summary>
//     /// 카드에 이 효과를 '장착'할 때 한 번만 호출됩니다.
//     /// 지정한 signal에 반응하는 Processor를 만들고, 계산 전용 ValueAction을 등록합니다.
//     /// </summary>
//     public override void Execute(SignalBus bus)
//     {
//         var card = bus?.GetSourceCard();
//         if (card == null) return;

//         // 1) 이 카드 소유의 평가 Processor 생성
//         var proc = new Processor(
//             sourceName: $"CostReducer({delta})",
//             isBase:     false,
//             owner:      card,
//             source:     card
//         );

//         // 2) 평가 시 실제 계산을 수행할 ValueAction 생성 (Add delta)
//         var calc = ScriptableObject
//             .CreateInstance<ValueAction>()
//             .Initialize(signal, CalcOp.Add, (object)delta);

//         // 3) 해당 signal에 등록
//         proc.RegisterAction(signal, calc);

//         // 4) 카드에 Processor 부착
//         card.AddProcessor(proc);
//     }
// }
