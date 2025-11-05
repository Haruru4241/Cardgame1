using UnityEngine;
using System.Collections.Generic;
using System.Linq; // LINQ Sum() 및 Last() 메서드를 위해 필요

[CreateAssetMenu(fileName = "NewRoundConfig", menuName = "Game/Round/Round Configuration")]
public class RoundConfig_SO : ScriptableObject
{
    [Header("General Round Settings")]
    public int roundsPerStage = 6; // 한 단막 당 라운드 수

    [Header("Stage Definitions")]
    public List<StageDefinition> stages = new List<StageDefinition>();

    
    public GameObject defaultEnemyPrefab;

    public StageDefinition GetStageDefinitionByGlobalRound(int globalRoundIndex)
    {
        // 1. 유효성 검사 (범위 확인)
        if (globalRoundIndex < 0 || globalRoundIndex >= stages.Count() * roundsPerStage)
        {
            Debug.LogError($"RoundConfig_SO: Global Round Index {globalRoundIndex} is out of defined range (0 to {stages.Count() * roundsPerStage - 1})!");
            return null; // 유효하지 않으면 null 반환
        }

        int stageIndex = globalRoundIndex / roundsPerStage; // 현재 단막 인덱스 (0, 1, 2, 3, 4)

        return stages[stageIndex];
    }

    // 각 단막(스테이지)의 정의
    [System.Serializable]
    public class StageDefinition
    {
        public string stageName;

        [Tooltip("1~5 라운드 중 일반 전투 그룹 (총 5개 필요, 인덱스 0~4)")]
        public List<EnemySpawnGroup> normalBattleGroups = new List<EnemySpawnGroup>(5);

        [Tooltip("3~5 라운드 중 엘리트 전투 그룹 (보통 1개만 정의)")]
        public List<EnemySpawnGroup> eliteBattleGroups = new List<EnemySpawnGroup>(1);

        [Tooltip("6 라운드 보스 전투 그룹 (1개)")]
        public EnemySpawnGroup bossBattleGroup;

        [Header("Elite Round Assignment in Stage (1-5 rounds)")]
        [Tooltip("0: 3~5 라운드(인덱스 2~4) 중 랜덤. \n1-5: 해당 라운드(1-5)에 엘리트 고정 할당.")]
        [Range(0, 5)] public int eliteRoundIndexInStage = 3; // 0이면 랜덤, 1~5는 고정 라운드 번호

        public int GetDeterminedEliteRoundIndex()
        {
            var EliteRoundIndex = eliteRoundIndexInStage;

            if (eliteRoundIndexInStage == 0) // 0은 무작위 선택을 의미
            {
                // 3~5 라운드 (1-indexed) 중 랜덤 선택
                // Random.Range(min, max)에서 int 타입은 max 미만의 값을 반환합니다.
                // 3, 4, 5 중 하나를 선택해야 하므로 Random.Range(3, 6)을 사용합니다.
                EliteRoundIndex = Random.Range(3, 6);
            }
            // 1~5 사이의 값이 반환되도록 보장 (StageDefinition 정의에 따라)
            return Mathf.Clamp(EliteRoundIndex, 1, 5);
        }
    }

    // 적 스폰 그룹 정의 (리스트 내 가중치 적용)
    [System.Serializable]
    public class EnemySpawnGroup
    {
        public string groupName;
        public List<WeightedEnemyData> enemies = new List<WeightedEnemyData>();
        public int initialSpawnCount = 50; // 이 라운드에서 스폰될 총 적 수

        // GetRandomEnemyData는 BattleManager에서 직접 사용할 수 있도록 유지.
        public EnemyData GetRandomEnemyData()
        {
            if (enemies == null || enemies.Count == 0)
            {
                Debug.LogWarning($"EnemySpawnGroup '{groupName}': No enemies defined in this group!");
                return null;
            }

            float totalWeight = enemies.Sum(e => e.weight);
            if (totalWeight <= 0)
            {
                Debug.LogWarning($"EnemySpawnGroup '{groupName}': Total weight is zero or negative. Returning first enemy as fallback.");
                return enemies[0].enemyData; // 가중치가 없으면 첫 번째 적 반환
            }

            float randomValue = Random.Range(0f, totalWeight);

            foreach (var weightedEnemy in enemies)
            {
                if (randomValue < weightedEnemy.weight)
                {
                    return weightedEnemy.enemyData;
                }
                randomValue -= weightedEnemy.weight;
            }
            return enemies.Last().enemyData; // 리스트의 마지막 요소 반환 (안전 장치)
        }
    }

    // 가중치가 부여된 적 데이터
    [System.Serializable]
    public class WeightedEnemyData
    {
        public EnemyData enemyData; // 사용자님의 EnemyData.cs
        [Range(0, 100)] public int weight = 100; // 가중치

        public int baseEnemyHPModifier = 100; // 적 HP 배율 (100 = 100%)
        public int baseEnemyArmorModifier = 100; // 적 Armor 배율 (100 = 100%)
    }

    public enum RoundType { Normal, Elite, Boss }

    // EnemyData 클래스 정의가 코드에 없었으므로 컴파일을 위해 임시로 추가합니다.
    // 사용자님의 실제 EnemyData 클래스로 대체되어야 합니다.
    public class EnemyData : ScriptableObject { /* 실제 적 데이터 정의 */ }
}