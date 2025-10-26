// Assets/Script/Actions/BusModifierAction.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// (BaseAction이 ScriptableObject이며, CreateBubble() 가상 메소드를 가진다고 가정합니다.)

[CreateAssetMenu(fileName = "New Bus Modifier", menuName = "Actions/Bus Modifier")]
public class BusModifierAction : BaseAction//익스큐트없으면 액셔느로
{
    [Header("1. 기본 설정")]
    [Tooltip("이 수정 액션의 실행 우선순위입니다. (낮을수록 먼저 실행됩니다)")]
    // 주석 피드백: "이거 낮을수록우선아닌가?"
    // -> 네, 맞습니다. 우선순위 시스템은 보통 낮은 숫자를 먼저 처리합니다.
    // -> 기본값을 100(높은 값)에서 0(낮은 값)으로 변경하고 툴팁을 수정합니다.
    public int priority = 1; // 낮은 값으로 설정 (예: 0)

    [Header("2. 버블 전송 설정")]
    [Tooltip("기존 버스의 남은 버블을 새 버스로 옮길 개수입니다. (0 = 이 버블 뒤의 '전체'를 의미)")]
    public int transferCount = 1; // 0 = 전체, 1 = 1개, N = N개

    [Header("3. 버블 수정 설정 (새 버스에 적용)")]
    [Tooltip("[변경1/변경2] 새 버스에 '추가'하거나 '교체'할 액션 목록입니다.")]
    public List<BaseAction> newActionsToAdd;

    [Tooltip("[변경2] 이 액션을 찾아서 'newActionsToAdd'로 교체합니다. (null이면 [변경1]처럼 맨 앞에 추가)")]
    public BaseAction actionToFind;

    public SignalType Evaluation;

    public override void Execute(SignalBus oldBus)
    {
        var a = new SignalBus(Evaluation);
        oldBus.GetSourceCard().PrepareBus(a);
        DeckManager.Instance.Rule.PrepareBus(a);
        ReactionStackManager.Instance.PushBus(a);

        int rawCount = CalculationManager.Instance.Evaluate<int>(oldBus, CalcType.Repeat); // (가정)
        int repeatCount = Mathf.Max(1, rawCount); // 최소 1회 실행 보장
        var busesToFire = new List<SignalBus>();

        // --- 3. 새 버스 생성 및 발사 (반복 횟수만큼) ---
        for (int i = 0; i < repeatCount; i++)
        {
            // [새 버스 생성]
            var newBus = new SignalBus(oldBus.Signal, oldBus.ParentBus); // (가정)
            newBus.SetSourceInfo(oldBus.GetSourceCard()); // (가정)
            newBus.Target = oldBus.Target;
            // (필요시 oldBus의 다른 정보도 newBus로 복사)

            newBus.AddPassengers(oldBus._bubbles
                                .Take((transferCount == 0) ? oldBus._bubbles.Count : Mathf.Min(transferCount, oldBus._bubbles.Count))
                                .Select(bubble => bubble.Clone())
                                .ToList()); // (가정)

            if (newActionsToAdd != null && newActionsToAdd.Count > 0)
            {
                var newActionsQueue = new Queue<BaseAction>();
                foreach (var action in newActionsToAdd)
                {
                    if (action != null)
                    {
                        newActionsQueue.Enqueue(action);
                    }
                }
                if (newActionsQueue.Count > 0)
                {
                    var singleBubble = new ActionBubble(newActionsQueue); // (가정)
                    newBus.AddPassenger(singleBubble);
                }
            }

            if (actionToFind != null)
            {
                // [변경2: 교체] (변경점 1 - 큐 내부 수정)
                bool replacementDone = false;
                foreach (var bubble in newBus._bubbles)
                {
                    // (가정) 'Actions'는 버블의 Queue<BaseAction> 속성
                    if (bubble._queue == null) continue;

                    Queue<BaseAction> currentQueue = bubble._queue;
                    Queue<BaseAction> modifiedQueue = new Queue<BaseAction>();
                    bool foundInThisBubble = false;

                    while (currentQueue.Count > 0)
                    {
                        var action = currentQueue.Dequeue();

                        if (!foundInThisBubble && action == actionToFind)
                        {
                            // 일치하는 액션을 찾음
                            foundInThisBubble = true;
                            replacementDone = true;

                            // 'newActionsToAdd' 리스트의 모든 액션을 큐에 삽입 (교체)
                            if (newActionsToAdd != null)
                            {
                                foreach (var newAction in newActionsToAdd)
                                {
                                    if (newAction != null)
                                        modifiedQueue.Enqueue(newAction);
                                }
                            }
                            // (찾은 'action'은 modifiedQueue에 넣지 않음으로써 '제거'됨)
                        }
                        else
                        {
                            // 일치하지 않는 액션은 그대로 다시 넣음
                            modifiedQueue.Enqueue(action);
                        }
                    }

                    // (가정) 버블의 큐를 수정된 큐로 교체
                    bubble._queue = modifiedQueue;

                    if (replacementDone)
                    {
                        break; // 버스 내 첫 번째 일치 항목만 교체하고 중단
                    }
                }
            }
            // [4-3. 새 버스 출발 준비]
            if (newBus.HasPassenger())
            {
                newBus.SortBubblesByPriority();

                // 변경점 1: 버스를 즉시 발사(Fire)하지 않고 리스트에 추가합니다.
                busesToFire.Add(newBus);
            }

        }
        if (busesToFire.Count > 0)
        {
            ReactionStackManager.Instance.PushBuses(busesToFire); // (가정)
        }
    }
}