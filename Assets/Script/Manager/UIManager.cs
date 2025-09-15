using UnityEngine;
using TMPro;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI 패널들")]
    public GameObject selectionPanel;
    public GameObject menuPanel;
    public GameObject animatingPanel;
    public GameObject gameOverPanel;

    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // 카드 선택 UI 표시/숨김
    public void ShowCardSelectionUI(bool show)
    {
        if (selectionPanel != null)
            selectionPanel.SetActive(show);
    }

    // 메뉴 UI 표시/숨김
    public void ShowMenu(bool show)
    {
        if (menuPanel != null)
            menuPanel.SetActive(show);
    }

    // 애니메이션 상태 UI 표시/숨김
    public void ShowAnimatingUI(bool show)
    {
        if (animatingPanel != null)
            animatingPanel.SetActive(show);
    }

    // 게임 오버 UI 표시/숨김
    public void ShowGameOverUI(bool show)
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(show);
    }
    public void SetScore(int value)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {value}";
    }
}
