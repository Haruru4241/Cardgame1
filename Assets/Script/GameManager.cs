using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public KeyCode useCardKey = KeyCode.Return;
    public KeyCode menuKey = KeyCode.Escape;
    public KeyCode cancelKey = KeyCode.R;
    public KeyCode endTurnKey = KeyCode.Space;

    public DeckPreset deckPreset;

    public GameStateBase MainState { get; private set; }
    public GameStateBase SelectState { get; private set; }
    public GameStateBase MenuState { get; private set; }

    public GameStateBase CurrentState { get; set; }
    public GameStateBase PreviousState { get; set; }

    public string _logs = "";
    [ContextMenu("▶ Show Signal–Processor Bindings")]
    public void DebugLogBindings()
    {
        Debug.Log(_logs);
    }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        MainState = new MainInputState(this);
        SelectState = new SelectState(this);
        MenuState = new MenuState(this);

        MainState.ChangeState(MainState); // 최초 상태
    }

    private void Start()
    {
        DeckManager.Instance.SetupDeck(deckPreset);
        TurnManager.Instance.StartTurn();
    }
    private void Update()
    {
        CurrentState?.Update();
    }
}
