using System.Collections.Generic;
using System;                  // Action 델리게이트를 위해
using System.Linq;             // ToList() 확장 메서드를 위해
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<ArtifactData> StartingArtifacts;
    public List<ArtifactInstance> ArtifactInstances { get; private set; } = new();

    public KeyCode useCardKey = KeyCode.Return;
    public KeyCode menuKey = KeyCode.Escape;
    public KeyCode cancelKey = KeyCode.R;
    public KeyCode endTurnKey = KeyCode.Space;

    public GamePreset deckPreset;

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
    }

    private void Start()
    {
        DeckManager.Instance.SetupGame(deckPreset);

        MainState.ChangeState(MainState); // 최초 상태
        ArtifactInstances = StartingArtifacts
            .Select(data => new ArtifactInstance(data))
            .ToList();
        TurnManager.Instance.StartTurn();

    }
    private void Update()
    {
        CurrentState?.Update();
    }
}
