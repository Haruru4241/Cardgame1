using System.Collections.Generic;
using UnityEngine;

// 입력 관리
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    private List<BaseController> selected = new List<BaseController>();

    private void Awake() => Instance = this;

    /// <summary>
    /// 현재 마우스 커서가 위에 있는 대상(카드, 적 등)입니다. 없으면 null 입니다.
    /// 다른 모든 시스템은 이 변수를 통해 '호버링된 대상' 정보를 얻습니다.
    /// </summary>
    public BaseInstance HoveredTarget { get; private set; }

    /// <summary>
    /// 외부(GameState)에서 호출하여 현재 호버링된 대상 정보를 갱신합니다.
    /// </summary>
    public void UpdateHoveredTarget(BaseInstance target)
    {
        HoveredTarget = target;
    }


    // public void OnCardHovered(BaseCard card)
    // {
    //     SelectCard(card);
    // }

    // public void OnCardUnhovered(BaseCard card)
    // {

    //     DeselectAll();
    // }

    // public BaseCard GetCardUnderMouse()
    // {
    //     // 2D UI라면 Raycast를 활용 (Canvas가 GraphicRaycaster 사용시)
    //     Vector2 mousePos = Input.mousePosition;
    //     Ray ray = Camera.main.ScreenPointToRay(mousePos);
    //     RaycastHit hit;

    //     if (Physics.Raycast(ray, out hit, 100f))
    //     {
    //         return hit.transform.GetComponent<BaseCard>();
    //     }
    //     return null;
    // }

    // public void SelectCard(BaseCard card)
    // {
    //     if (card == null) return;
    //     DeselectAll();  // 하나만 선택하는 경우, 여러 장이면 주석처리
    //     selected.Add(card);
    //     card.cardInstance.Fire(SignalType.OnSelect);
    // }

    // public bool IsSelectionComplete()
    // {
    //     return selected.Count == 1; // 여러 장이면 원하는 개수로 변경!
    // }

    // public void OnCardClicked(BaseCard card)
    // {
    //     if (selected.Contains(card))  // **변경**: 즉시 사용 모드거나 이미 선택된 카드라면
    //     {
    //         card.UseCard();                            // **변경**: 즉시 사용 모드 해제
    //         DeselectAll();                              // **변경**: 선택 해제
    //     }
    //     else
    //     {
    //         DeselectAll();
    //         selected.Add(card);
    //         card.cardInstance.Fire(SignalType.OnSelect);
    //     }
    // }

    // public void UseSelected()
    // {
    //     foreach (var c in selected)
    //         c.cardInstance.Fire(SignalType.OnUse);
    //     selected.Clear();
    // }

    // public void DeselectAll()
    // {
    //     foreach (var c in selected)
    //         c.cardInstance.Fire(SignalType.OnUnSelect);
    //     selected.Clear();
    // }

    // public void HandleShortcuts()
    // {
    //     for (int i = 1; i <= 8; i++)
    //         if (Input.GetKeyDown(i.ToString()))
    //         {
    //             var pileCards = DeckManager.Instance.HandPile.Cards;
    //             if (i - 1 < pileCards.Count)
    //                 OnCardClicked(pileCards[i - 1].BaseCard);
    //         }
    // }
}
