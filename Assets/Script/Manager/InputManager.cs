using System.Collections.Generic;
using UnityEngine;

// 입력 관리
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;
    private List<BaseCard> selected = new List<BaseCard>();

    private void Awake() => Instance = this;

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
