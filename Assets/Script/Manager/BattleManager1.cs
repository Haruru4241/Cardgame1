using UnityEngine;
using System.Collections.Generic;
using System.Linq; 

// **[수정 1]** 클래스 이름 변경: RoundManager -> BattleManager
public class BattleManager1 : MonoBehaviour
{
    // BattleManager가 GameManager와 통신하기 위해 GameManager의 싱글톤 인스턴스를 가정합니다.
    // public GameManager GameManagerInstance; // 혹은 싱글톤 패턴으로 접근

    [Header("Configuration")]
    [Tooltip("라운드 데이터를 담고 있는 ScriptableObject")]
    public RoundConfig_SO roundConfig;
    
    // **[수정 3]** 현재 라운드가 속한 StageDefinition을 저장
    private RoundConfig_SO.StageDefinition currentStageDef;

    [Header("Runtime Status")]
    // 현재 진행 중인 글로벌 턴 인덱스 (0부터 시작, 0~29)
    // 사용자 정의: 총 30 스테이지 (턴) 중 현재 진행 인덱스
    [SerializeField] private int currentGlobalRoundIndex = 0; 
    
    // 현재 필드에 존재하는 적 수 (클리어 조건 확인용)
    private int enemiesAliveCount = 0; 

    // 적 스폰 지점 (유니티 에디터에서 할당)
    public Transform enemySpawnPoint; 
    
    // **[수정 2]** 모든 C# 이벤트 제거됨

    // --- 3. 핵심 메서드 ---

    private void Start()
    {
        if (roundConfig == null)
        {
            Debug.LogError("RoundConfig_SO is not assigned to the BattleManager!");
        }
    }

    /// <summary>
    /// **턴 (Global Round Index)**을 시작하여 적을 스폰합니다. (전투 턴 시작)
    /// 이 메서드는 **GameManager**에 의해 호출됩니다.
    /// </summary>
    public void StartCombatTurn()
    {
        int totalRounds = roundConfig.stages.Count() * roundConfig.roundsPerStage;
        if (currentGlobalRoundIndex >= totalRounds)
        {
            // **[수정 2, 3 반영]** 게임 클리어 시 GameManager의 메서드 호출 가정
            Debug.Log("Game Cleared! All combat turns finished.");
            // GameManager.Instance.EndGame(); // GameManager의 게임 종료 메서드 호출 가정
            return;
        }

        currentStageDef = roundConfig.GetStageDefinitionByGlobalRound(currentGlobalRoundIndex);
        if (currentStageDef == null)
        {
            Debug.LogError($"Failed to get Stage Definition for Global Round {currentGlobalRoundIndex + 1}.");
            return;
        }
        
        // 변수 초기화
        enemiesAliveCount = 0;
        
        Debug.Log($"--- Starting Combat Turn {currentGlobalRoundIndex + 1} (Stage: {currentStageDef.stageName}) ---");
        // TODO: 전투 시작 알림을 GameManager나 UIManager에 직접 전달 (메서드 호출 또는 이벤트로 대체)
        
        SpawnAllEnemies();
    }

    /// <summary>
    /// 해당 턴에 스폰해야 할 모든 적 그룹의 적을 한 번에 스폰합니다.
    /// </summary>
    private void SpawnAllEnemies()
    {
        // ... (스폰 그룹을 계산하는 로직은 이전과 동일하게 유지)
        int roundInStageIndex = currentGlobalRoundIndex % roundConfig.roundsPerStage;
        List<RoundConfig_SO.EnemySpawnGroup> spawnGroups = GetSpawnGroupsForCurrentTurn(roundInStageIndex);

        if (spawnGroups.Count == 0)
        {
            Debug.LogWarning($"No spawn groups found for Global Round {currentGlobalRoundIndex + 1}. Ending combat immediately.");
            EndCombatTurn(); // 적이 없으면 바로 다음 턴으로
            return;
        }
        
        int totalCount = 0;
        foreach (var group in spawnGroups)
        {
            for (int i = 0; i < group.initialSpawnCount; i++)
            {
                RoundConfig_SO.EnemyData enemyData = group.GetRandomEnemyData();
                
                if (enemyData != null)
                {
                    // **[참고]** 실제 스폰 로직 (프리팹 인스턴스화 및 초기화)
                    GameObject newEnemy = new GameObject("Enemy"); 
                    newEnemy.transform.position = enemySpawnPoint.position;
                    
                    enemiesAliveCount++; 
                    totalCount++;

                    // **중요:** 적 사망 시 이 BattleManager의 EnemyDiedNotification()을 호출하도록 설정해야 합니다.
                }
            }
        }
        Debug.Log($"Total {totalCount} enemies spawned for combat turn {currentGlobalRoundIndex + 1}.");
    }

    /// <summary>
    /// StageDefinition에서 현재 턴에 해당하는 적 스폰 그룹 리스트를 반환합니다.
    /// </summary>
    private List<RoundConfig_SO.EnemySpawnGroup> GetSpawnGroupsForCurrentTurn(int roundInStageIndex)
    {
        List<RoundConfig_SO.EnemySpawnGroup> groups = new List<RoundConfig_SO.EnemySpawnGroup>();

        if (currentStageDef == null) return groups;

        // 1. 보스 턴 (인덱스 5, 6번째 턴) 처리
        if (roundInStageIndex == 5)
        {
            if (currentStageDef.bossBattleGroup != null)
            {
                groups.Add(currentStageDef.bossBattleGroup);
            }
        }
        // 2. 일반/엘리트 턴 (인덱스 0~4) 처리
        else if (roundInStageIndex >= 0 && roundInStageIndex < currentStageDef.normalBattleGroups.Count)
        {
            // 일반 그룹 추가
            if (currentStageDef.normalBattleGroups[roundInStageIndex] != null)
            {
                groups.Add(currentStageDef.normalBattleGroups[roundInStageIndex]);
            }

            // 엘리트 라운드인지 확인 후 엘리트 그룹 추가
            int determinedEliteRoundIndex = currentStageDef.GetDeterminedEliteRoundIndex() - 1;

            if (roundInStageIndex == determinedEliteRoundIndex)
            {
                if (currentStageDef.eliteBattleGroups != null)
                {
                    groups.AddRange(currentStageDef.eliteBattleGroups.Where(g => g != null));
                }
            }
        }
        return groups;
    }


    /// <summary>
    /// 적이 사망했을 때 외부 (Enemy 스크립트)에서 호출되어야 하는 함수입니다.
    /// </summary>
    public void EnemyDiedNotification()
    {
        enemiesAliveCount--;
        
        // 라운드 클리어 조건 확인: 필드의 모든 적이 죽었을 때
        if (enemiesAliveCount <= 0)
        {
            Debug.Log($"All enemies defeated in combat turn {currentGlobalRoundIndex + 1}!");
            EndCombatTurn();
        }
    }

    /// <summary>
    /// 전투 턴이 끝났음을 알리고 다음 상점 턴으로 전환하도록 GameManager에 요청합니다.
    /// </summary>
    private void EndCombatTurn()
    {
        // 다음 턴을 준비합니다.
        currentGlobalRoundIndex++; 
        
        // **[수정 3 반영]** 전투가 끝났으므로 GameManager의 다음 단계(상점 턴) 시작 메서드를 호출합니다.
        // 이 부분은 사용자님의 GameManager 구조에 따라 수정해야 합니다.
        // GameManager.Instance.StartShopPhase(); // GameManager의 메서드 호출 가정
        
        Debug.Log($"Combat Turn Ended. Requesting GameManager to start Shop Turn.");
    }
}