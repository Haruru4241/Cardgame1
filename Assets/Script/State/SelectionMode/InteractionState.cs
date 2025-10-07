using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;
public class InteractionState : GameStateBase
{
    private SelectionMode _currentModeSO;
    // ★★★ 부활: 후보 목록을 가져오는 '함수' ★★★
    private Action<List<BaseInstance>> _onCompleteCallback;
    private SignalBus _selectionBus;

    // ★★★ Candidates는 이제 Enter 시점에 계산됩니다. ★★★
    public List<BaseInstance> Candidates { get; private set; }
    public int RequiredCount { get; private set; }
    public List<BaseInstance> SelectedTargets { get; private set; } = new List<BaseInstance>();

    public InteractionState(GameManager manager) : base(manager) { }

    public void StartSelection(
        SelectionMode modeSO,
        // ★★★ 부활: 후보 목록 대신, 후보 목록을 가져오는 '방법'을 받습니다. ★★★
        Func<List<BaseInstance>> getCandidatesFunc,
        int requiredCount,
        Action<List<BaseInstance>> onComplete,
        SignalBus bus)
    {
        if (GameManager.Instance.CurrentState == this) { Debug.Log("재진입 에러"); return; }
        _selectionBus = bus;
        _selectionBus.TryTakeToken();

        this._currentModeSO = modeSO;
        this._onCompleteCallback = onComplete;
        this.Candidates = getCandidatesFunc();
        this.RequiredCount = Mathf.Min(Candidates.Count, requiredCount);

        if (Candidates == null || Candidates.Count == 0)
        {
            Debug.LogWarning("선택 후보가 없습니다!");
            ReturnToPreviousState();
            return;
        }
        GameManager.Instance._logs += " 선택 모드 진입 ";
        ChangeState(this);
    }

    public override void Enter()
    {
        SelectedTargets.Clear();
        // ★★★ 핵심: 상태 진입 직후, '함수'를 실행하여 최신 후보 목록을 가져옵니다. ★★★

        _currentModeSO?.OnEnter(this);
    }

    public override void Update()
    {
        _currentModeSO?.OnUpdate(this);
    }

    public override void Exit()
    {
        _currentModeSO?.OnExit(this);
    }

    public void CompleteSelection(List<BaseInstance> finalSelection)
    {
        GameManager.Instance._logs += " 선택 모드 탈출 ";
        var result = _confirmed.Select(bc => bc.baseInstance).ToList();
        var onSelected = _onCompleteCallback;
        ChangeState(GameManager.Instance.MainState);

        _selectionBus?.ReturnToken();

        onSelected?.Invoke(result);

        ReactionStackManager.Instance.StartProcessing();
    }
    public void OnCardHovered(BaseController bc)
    {
        // 후보 카드만 처리
        if (Candidates.Contains(bc.baseInstance) && CanSelectMore())
        {
            selected.Add(bc);
            bc.baseInstance.Fire(new SignalBus(SignalType.OnSelect));
        }
    }

    public void OnCardUnhovered(BaseController bc)
    {
        if (Candidates.Contains(bc.baseInstance) && selected.Contains(bc))
        {
            selected.Remove(bc);
            bc.baseInstance.Fire(new SignalBus(SignalType.OnUnSelect));
        }
    }

    public void OnCandidateClicked(BaseController bc)
    {
        // 후보가 아닌데 이미 확정된 카드라면 → 해제
        if (!Candidates.Contains(bc.baseInstance) && _confirmed.Contains(bc))
        {
            _confirmed.Remove(bc);
            bc.SetHighlight(true, Color.red);
            bc.baseInstance.Fire(new SignalBus(SignalType.OnUnSelect));
            Candidates.Add(bc.baseInstance); // 다시 후보에 넣어주기
            return;
        }

        // 새로 선택하는 경우
        if (Candidates.Contains(bc.baseInstance))
        {
            OnCardUnhovered(bc);
            _confirmed.Add(bc);
            Candidates.Remove(bc.baseInstance);
            bc.SetHighlight(true, Color.blue);

            // 원하는 개수까지 모였으면 완료
            if (_confirmed.Count >= RequiredCount)
                CompleteSelection(selected.Select(bc => bc.baseInstance).ToList());
        }
    }
    public void Cleanup()
    {
        // 1) UI 끄기
        UIManager.Instance.ShowCardSelectionUI(false);

        // 2) 후보 하이라이트 해제 & 이벤트 해제
        if (Candidates != null)
        {
            foreach (var bc in Candidates)
            {
                bc.controller.SetHighlight(false, Color.clear);
            }
        }
        if (_confirmed != null)
        {
            foreach (var bc in _confirmed)
            {
                bc.baseInstance.Fire(new SignalBus(SignalType.OnUnSelect));
                bc.SetHighlight(false, Color.clear);
            }
            _confirmed.Clear();
        }
        if (selected != null)
        {
            foreach (var bc in selected)
            {
                bc.baseInstance.Fire(new SignalBus(SignalType.OnUnSelect));
            }
            selected.Clear();
        }
        // 3) 내부 상태 리셋
        Candidates = null;
        _onCompleteCallback = null;
    }
    public bool CanSelectMore()
    {
        return selected.Count + _confirmed.Count < RequiredCount;
    }
}