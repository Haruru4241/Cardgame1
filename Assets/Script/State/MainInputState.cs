using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class MainInputState : GameStateBase
{
    public MainInputState(GameManager manager) : base(manager) { }

    public override void Enter()
    {
        var dm = DeckManager.Instance;
        dm.ReloadCustomUI(dm.GetPile(PileType.Hand).Cards);
        // 카드 클릭/호버 이벤트 구독
        BaseCard.OnCardClicked += HandleCardClicked;
        BaseCard.OnCardHovered += HandleCardHovered;
        BaseCard.OnCardUnhovered += HandleCardUnhovered;
    }

    public override void Exit()
    {
        // 꼭 해제해 줘야 다른 상태로 넘어갈 때 이벤트가 중복되지 않습니다
        BaseCard.OnCardClicked -= HandleCardClicked;
        BaseCard.OnCardHovered -= HandleCardHovered;
        BaseCard.OnCardUnhovered -= HandleCardUnhovered;
    }

    public override void Update()
    {
        if (Input.GetKeyDown(gameManager.menuKey))
            ChangeState(gameManager.MenuState);

        if (Input.GetKeyDown(gameManager.useCardKey))
            UseSelectedCards();

        if (Input.GetKeyDown(gameManager.cancelKey))
            DeselectAll();

        if (Input.GetKeyDown(gameManager.endTurnKey))
        {
            TurnManager.Instance.EndTurn();
            DeselectAll();
        }

        // 숫자 단축키 같은 나머지 입력은 그대로 처리
        HandleShortcuts();
    }

}