using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
public abstract class GameStateBase
{
    protected GameManager gameManager;
    public GameStateBase(GameManager manager) { gameManager = manager; }
    protected List<BaseCard> selected = new List<BaseCard>();
    protected List<BaseCard> _confirmed = new List<BaseCard>(); // 최종 확정 선택

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

    public void ChangeState(GameStateBase newState)
    {
        if (GameManager.Instance.CurrentState != null)
            GameManager.Instance.CurrentState.Exit();

        gameManager.PreviousState = gameManager.CurrentState;
        gameManager.CurrentState = newState;

        if (GameManager.Instance.CurrentState != null)
            GameManager.Instance.CurrentState.Enter();
    }
    public void ReturnToPreviousState()
    {
        if (GameManager.Instance.PreviousState != null)
            ChangeState(GameManager.Instance.PreviousState);
    }
    public virtual void HandleShortcuts()
    {
        for (int i = 1; i <= 8; i++)
            if (Input.GetKeyDown(i.ToString()))
            {
                var pileCards = DeckManager.Instance.HandPile.Cards;
                if (i - 1 < pileCards.Count)
                    OnCardClicked(pileCards[i - 1].BaseCard);
            }
    }
    public void OnCardClicked(BaseCard card)
    {
        if (selected.Contains(card))  // **변경**: 즉시 사용 모드거나 이미 선택된 카드라면
        {
            card.UseCard();                            // **변경**: 즉시 사용 모드 해제
            DeselectAll();                              // **변경**: 선택 해제
        }
        else
        {
            DeselectAll();
            selected.Add(card);
            card.cardInstance.Fire(SignalType.OnSelect);
        }
    }
    public void DeselectAll()
    {
        foreach (var c in selected)
            c.cardInstance.Fire(SignalType.OnUnSelect);
        selected.Clear();
        _confirmed.Clear();
    }
    protected virtual void HandleCardClicked(BaseCard bc)
    {
        // 클릭 시 즉시 사용
        OnCardClicked(bc);
        //bc.cardInstance.Fire(SignalType.OnUse);
    }

    protected virtual void HandleCardHovered(BaseCard bc)
    {
        DeselectAll();
        selected.Add(bc);
        bc.cardInstance.Fire(SignalType.OnSelect);
    }

    protected virtual void HandleCardUnhovered(BaseCard bc)
    {
        if (selected.Contains(bc))  // **변경**: 즉시 사용 모드거나 이미 선택된 카드라면
        {
            selected.Remove(bc);
            bc.cardInstance.Fire(SignalType.OnUnSelect);                           // **변경**: 선택 해제
        }
    }
    protected virtual void UseSelectedCards()
    {
        foreach (var c in selected)
            c.UseCard();     
        selected.Clear();
    }
}

public class MenuState : GameStateBase
{
    public MenuState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        UIManager.Instance.ShowMenu(true);
        Time.timeScale = 0f;
    }

    public override void Update()
    {
        if (Input.GetKeyDown(GameManager.Instance.cancelKey))
        {
            UIManager.Instance.ShowMenu(false);
            Time.timeScale = 1f;
            ReturnToPreviousState();
        }
    }

    public override void Exit()
    {
        UIManager.Instance.ShowMenu(false);
        Time.timeScale = 1f;
    }
}

public class EnemyTurnState : GameStateBase
{
    public EnemyTurnState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("적 턴 시작");
        // AI 행동 시작 등
    }

    public override void Update()
    {
        // 적 행동 등
    }

    public override void Exit()
    {
        Debug.Log("적 턴 종료");
        // 적 행동 정리 등
    }
}

public class AnimatingState : GameStateBase
{
    public AnimatingState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("애니메이션 단계 진입");
        UIManager.Instance.ShowAnimatingUI(true);
        // 코루틴은 GameManager에서 실행
        gameManager.StartCoroutine(WaitForAnimation());
    }

    private IEnumerator WaitForAnimation()
    {
        yield return new WaitForSeconds(1f);
        ChangeState(gameManager.MainState);
    }

    public override void Exit()
    {
        Debug.Log("애니메이션 단계 종료");
        UIManager.Instance.ShowAnimatingUI(false);
    }
}

public class GameOverState : GameStateBase
{
    public GameOverState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        Debug.Log("게임 오버!");
        UIManager.Instance.ShowGameOverUI(true);
    }

    public override void Update()
    {
        if (Input.GetKeyDown(GameManager.Instance.useCardKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (Input.GetKeyDown(GameManager.Instance.cancelKey))
        {
            SceneManager.LoadScene("TitleScene");
        }
    }

    public override void Exit()
    {
        Debug.Log("게임 오버 단계 종료");
        UIManager.Instance.ShowGameOverUI(false);
    }
}

