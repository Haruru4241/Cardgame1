using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Unity.VisualScripting;


public class SelectState : GameStateBase
{
    private List<BaseInstance> _candidateInstances;
    private int _requiredCount;
    private Action<List<BaseInstance>> _onSelected;
    public SelectState(GameManager manager) : base(manager) { }
    private SignalBus _selectionBus;


    public void StartSelection(
    List<BaseInstance> candidates,
    int requiredCount,
    Action<List<BaseInstance>> onSelected,
    SignalBus bus                  // 🔹 버스 받기
)
    {
        if (candidates == null || candidates.Count == 0 || requiredCount > candidates.Count)
        {
            Debug.LogWarning("선택 후보가 없습니다!");
            ReturnToPreviousState();
            return;
        }
        if (GameManager.Instance.CurrentState == this) Debug.Log("재진입 에러");

        _candidateInstances = candidates;
        _requiredCount = Mathf.Max(1, requiredCount);
        _onSelected = onSelected;
        selected.Clear();

        _selectionBus = bus;
        _selectionBus.TryTakeToken();

        ChangeState(this);
        GameManager.Instance._logs += " 선택 모드 진입 ";
    }

    private void CompleteSelection()
    {
        var result = _confirmed.Select(bc => bc.cardInstance).ToList();
        var onSelected = _onSelected;
        GameManager.Instance._logs += "선택 모드 탈출";

        ChangeState(GameManager.Instance.MainState);

        // 🔹 토큰 반환 및 처리 재개
        _selectionBus?.ReturnToken();
        _selectionBus = null;

        onSelected?.Invoke(result);

        // 다음 스텝 진행
        ReactionStackManager.Instance.StartProcessing();
    }


    public override void Enter()
    {
        // 1) 선택 UI 켜기
        UIManager.Instance.ShowCardSelectionUI(true);

        var dm = DeckManager.Instance;
        dm.ReloadCustomUI(_candidateInstances);

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
        if (GameManager.Instance.CurrentState == this)
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
                    bc.cardInstance.Fire(new SignalBus(SignalType.OnUnSelect));
                    break;
                }

                // 3) 새로 선택: 가능 여부 검사
                if (CanSelectMore())
                {
                    selected.Add(bc);
                    bc.cardInstance.Fire(new SignalBus(SignalType.OnSelect));

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
            bc.cardInstance.Fire(new SignalBus(SignalType.OnSelect));
        }
    }

    private void OnCardUnhovered(BaseCard bc)
    {
        if (_candidateInstances.Contains(bc.cardInstance) && selected.Contains(bc))
        {
            selected.Remove(bc);
            bc.cardInstance.Fire(new SignalBus(SignalType.OnUnSelect));
        }
    }

    private void OnCandidateClicked(BaseCard bc)
    {
        // 후보가 아닌데 이미 확정된 카드라면 → 해제
        if (!_candidateInstances.Contains(bc.cardInstance) && _confirmed.Contains(bc))
        {
            _confirmed.Remove(bc);
            bc.SetHighlight(false, Color.clear);
            bc.cardInstance.Fire(new SignalBus(SignalType.OnUnSelect));
            _candidateInstances.Add(bc.cardInstance); // 다시 후보에 넣어주기
            return;
        }

        // 새로 선택하는 경우
        if (_candidateInstances.Contains(bc.cardInstance))
        {
            OnCardUnhovered(bc);
            _confirmed.Add(bc);
            _candidateInstances.Remove(bc.cardInstance);
            bc.SetHighlight(true, Color.blue);

            // 원하는 개수까지 모였으면 완료
            if (_confirmed.Count >= _requiredCount)
                CompleteSelection();
        }
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
                bc.cardInstance.Fire(new SignalBus(SignalType.OnUnSelect));
                bc.SetHighlight(false, Color.clear);
            }
            _confirmed.Clear();
        }
        if (selected != null)
        {
            foreach (var bc in selected)
            {
                bc.cardInstance.Fire(new SignalBus(SignalType.OnUnSelect));
            }
            selected.Clear();
        }
        // 3) 내부 상태 리셋
        _candidateInstances = null;
        _onSelected = null;
    }
}

