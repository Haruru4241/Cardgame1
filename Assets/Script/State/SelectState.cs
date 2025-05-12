using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;


public class SelectState : GameStateBase
{
    private List<CardInstance> _candidateInstances;
    private int _requiredCount;
    private Action<List<CardInstance>> _onSelected;
    public SelectState(GameManager manager) : base(manager) { }
    //public Token _token;

    public void StartSelection(
        List<CardInstance> candidates,
        int requiredCount,
        Action<List<CardInstance>> onSelected
    )
    {
        GameManager.Instance._logs += string.Format(" 선택 모드 진입 ");
        if (GameManager.Instance.CurrentState == GameManager.Instance.SelectState)
        {
            ChangeState(GameManager.Instance.MainState);
            Debug.Log("선택 모드 재진입 오류");
        }
        if (candidates == null || candidates.Count == 0)
        {
            Debug.LogWarning("선택 후보가 없습니다!");
            ReturnToPreviousState();
            return;
        }
        ReactionStackManager.Instance.CurrentProcessor.UpdateHandlerToken(this);
        //ReactionStackManager.Instance._managerToken.UpdateToken(this);

        _candidateInstances = candidates;
        _requiredCount = Mathf.Max(1, requiredCount);
        _onSelected = onSelected;
        selected.Clear();

        ChangeState(GameManager.Instance.SelectState);
    }
    private void CompleteSelection()
    {
        // 1) 선택된 BaseCard → CardInstance 리스트로
        var result = _confirmed
            .Select(bc => bc.cardInstance)
            .ToList();
        var onSelected = _onSelected;
        ReactionStackManager.Instance.CurrentProcessor.UpdateHandlerToken(ReactionStackManager.Instance.CurrentProcessor);

        ChangeState(GameManager.Instance.MainState);
        // 2) 콜백 호출
        GameManager.Instance._logs += string.Format(" 선택 모드 종료 ");
        onSelected?.Invoke(result);
        ReactionStackManager.Instance.ProcessNext();
        //_token.InvokeIfSource(this);
        //ReactionStackManager.Instance.CurrentProcessor.ConsumeHandlerToken(this);
    }

    public override void Enter()
    {
        // 1) 선택 UI 켜기
        UIManager.Instance.ShowCardSelectionUI(true);

        // 2) 후보 하이라이트 & 이벤트 구독
        foreach (var bc in _candidateInstances)
        {
            bc.BaseCard.SetHighlight(true, Color.red);
        }
        BaseCard.OnCardClicked += OnCandidateClicked;
        BaseCard.OnCardHovered += OnCardHovered;
        BaseCard.OnCardUnhovered += OnCardUnhovered;
    }
    public override void Exit()
    {
        BaseCard.OnCardClicked -= OnCandidateClicked;
        BaseCard.OnCardHovered -= OnCardHovered;
        BaseCard.OnCardUnhovered -= OnCardUnhovered;

        // 혹시 클린업이 안 됐다면 안전하게 한 번 더
        Cleanup();
    }

    public override void Update()
    {
        if (GameManager.Instance.CurrentState != this)
            return;
        if (Input.GetKeyDown(gameManager.menuKey))
            ChangeState(gameManager.MenuState);

        // 선택 확정(Use키): preview -> confirmed
        // 2) useCardKey 처리 로직 (Update 메서드 내)
        if (Input.GetKeyDown(gameManager.useCardKey) && selected.Count > 0)
        {
            // preview에 남은 카드를 모두 OnCandidateClicked로 처리
            foreach (var bc in selected.ToList())
            {
                OnCandidateClicked(bc);
            }
            selected.Clear();
        }


        if (Input.GetKeyDown(gameManager.endTurnKey))
        {
            ChangeState(gameManager.MainState);
            TurnManager.Instance.EndTurn();
        }
        HandleShortcuts();
    }

    // 1) HandleShortcuts 메서드
    public override void HandleShortcuts()
    {
        for (int i = 1; i <= Mathf.Min(_candidateInstances.Count, 9); i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                var ci = _candidateInstances[i - 1];
                var bc = ci.BaseCard;

                // 2) preview에 이미 있으면 토글 해제
                if (selected.Contains(bc))
                {
                    selected.Remove(bc);
                    bc.cardInstance.Fire(SignalType.OnUnSelect);
                    break;
                }

                // 3) 새로 선택: 가능 여부 검사
                if (CanSelectMore())
                {
                    selected.Add(bc);
                    bc.cardInstance.Fire(SignalType.OnSelect);

                    // 4) total이 requiredCount에 도달하면 즉시 확정
                    if (selected.Count + _confirmed.Count == _requiredCount)
                    {
                        foreach (var sel in selected.ToList())
                            OnCandidateClicked(sel);
                        selected.Clear();
                    }
                }
                break;
            }
        }
    }

    private void OnCardHovered(BaseCard bc)
    {
        // 후보 카드만 처리
        if (_candidateInstances.Contains(bc.cardInstance) && CanSelectMore())
        {
            selected.Add(bc);
            bc.cardInstance.Fire(SignalType.OnSelect);
        }
    }

    private void OnCardUnhovered(BaseCard bc)
    {
        if (_candidateInstances.Contains(bc.cardInstance) && selected.Contains(bc))
        {
            selected.Remove(bc);
            bc.cardInstance.Fire(SignalType.OnUnSelect);
        }
    }

    private void OnCandidateClicked(BaseCard bc)
    {
        // 중복 선택 방지
        if (!_candidateInstances.Contains(bc.cardInstance) && _confirmed.Contains(bc)) return;

        // 1) 선택 추가 & 하이라이트 색 바꾸기
        OnCardUnhovered(bc);
        _confirmed.Add(bc);
        _candidateInstances.Remove(bc.cardInstance);
        bc.SetHighlight(true, Color.blue);

        // 2) 원하는 개수까지 모였으면 완료
        if (_confirmed.Count >= _requiredCount)
            CompleteSelection();
    }


    // 최대 선택 가능 여부 체크
    private bool CanSelectMore()
    {
        return selected.Count + _confirmed.Count < _requiredCount;
    }

    private void Cleanup()
    {
        // 1) UI 끄기
        UIManager.Instance.ShowCardSelectionUI(false);

        // 2) 후보 하이라이트 해제 & 이벤트 해제
        if (_candidateInstances != null)
        {
            foreach (var bc in _candidateInstances)
            {
                bc.BaseCard.SetHighlight(false, Color.clear);
            }
        }
        if (_confirmed != null)
        {
            foreach (var bc in _confirmed)
            {
                bc.cardInstance.Fire(SignalType.OnUnSelect);
                bc.SetHighlight(false, Color.clear);
            }
            _confirmed.Clear();
        }
        if (selected != null)
        {
            foreach (var bc in selected)
            {
                bc.cardInstance.Fire(SignalType.OnUnSelect);
            }
            selected.Clear();
        }
        // 3) 내부 상태 리셋
        _candidateInstances = null;
        _onSelected = null;
    }
}

