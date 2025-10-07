using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
/// <summary>
/// 🎯 필드 직접 지정 (조준선) 방식의 로직과 설정을 정의하는 SO입니다.
/// </summary>
[CreateAssetMenu(fileName = "FieldTargetingMode", menuName = "CardGame/Selection Modes/Field Targeting")]
public class FieldTargetingMode : SelectionMode
{
    public override void OnEnter(InteractionState state)
    {
        UIManager.Instance.ShowCardSelectionUI(true);

        // 2) 후보 하이라이트 & 이벤트 구독
        foreach (var bc in state.Candidates)
        {
            bc.controller?.SetHighlight(true, Color.red);
        }
        BaseController.OnEntityClicked += state.OnCandidateClicked;
        BaseController.OnEntityHovered += state.OnCardHovered;
        BaseController.OnEntityUnhovered += state.OnCardUnhovered;
    }

    public override void OnUpdate(InteractionState state)
    {
        // 조준선 UI를 마우스 따라 움직이는 로직...

        // if (Input.GetMouseButtonUp(0))
        // {
        //     BaseInstance target = FindTargetUnderMouse();
        //     if (target != null && state.Candidates.Contains(target))
        //     {
        //         state.SelectedTargets.Add(target);
        //         if (state.SelectedTargets.Count >= state.RequiredCount)
        //         {
        //             state.CompleteSelection(state.SelectedTargets);
        //         }
        //     }
        //     else
        //     {
        //         state.CompleteSelection(null); // 실패(취소)
        //     }
        // }
    }

    public override void OnExit(InteractionState state)
    {
        Debug.Log("Field Targeting Mode: Exit");
        // 모든 후보들의 하이라이트를 끕니다.
        foreach (var candidate in state.Candidates)
        {
            if (candidate != null && candidate.controller != null)
                candidate.controller.SetHighlight(false, Color.clear);
        }
        BaseController.OnEntityClicked -= state.OnCandidateClicked;
        BaseController.OnEntityHovered -= state.OnCardHovered;
        BaseController.OnEntityUnhovered -= state.OnCardUnhovered;

        // 혹시 클린업이 안 됐다면 안전하게 한 번 더
        state.Cleanup();
    }

    private BaseInstance FindTargetUnderMouse()
    {
        // 프로젝트에 맞는 레이캐스트 로직
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<BaseController>()?.baseInstance;
        }
        return null;
    }
}