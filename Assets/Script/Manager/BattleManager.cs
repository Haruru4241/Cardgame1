using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// 전투의 시작과 종료, 참여자(플레이어, 적) 생성을 관리합니다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    [Header("참여자 프리펩")]
    public GameObject playerPrefab; // 플레이어 캐릭터 프리펩
    public GameObject enemyPrefab;  // 적 캐릭터 프리펩

    [Header("현재 전투 정보")]
    public List<EnemyInstance> enemyInstances = new List<EnemyInstance>(); // 여러 적 인스턴스를 관리


    [Header("참여자 배치 Zone")]
    public Zone playerZone = new PlayerZone(); // 플레이어가 위치할 Zone
    public Zone enemyZone = new EnemyZone();  // 적이 위치할 Zone
    public Transform playerArea;
    public Transform enemyArea;

    [Header("초기 전투 설정")]
    // 단일 데이터에서 리스트로 변경
    public List<EnemyData> initialEnemyWave;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// 지정된 적 데이터 웨이브로 전투를 시작하고 씬에 모든 적을 생성합니다.
    /// </summary>
    public void StartBattle()
    {
        if (initialEnemyWave == null || initialEnemyWave.Count == 0)
        {
            Debug.LogError("전투를 시작할 적 데이터가 없습니다!");
            return;
        }

        // 기존에 있던 적들 정보 초기화
        enemyInstances.Clear();

        // 리스트에 있는 모든 적 데이터를 순회하며 생성
        foreach (var enemyData in initialEnemyWave)
        {
            // 1. 적의 논리적 인스턴스를 생성합니다.
            var newEnemyInstance = new EnemyInstance(enemyData);

            // 2. 적 프리펩을 enemyZone의 자식으로 생성합니다.
            GameObject enemyObject = Instantiate(enemyPrefab, enemyArea); // Zone의 Transform을 부모로 지정
            EnemyController enemyController = enemyObject.GetComponent<EnemyController>();

            // 3. 생성된 인스턴스와 컨트롤러를 서로 연결합니다.
            enemyController.Setup(enemyData, newEnemyInstance);
            newEnemyInstance.controller = enemyController;

            // 4. Zone에 인스턴스를 추가하고, 리스트에서 관리합니다.
            enemyZone.Add(newEnemyInstance);
            enemyInstances.Add(newEnemyInstance);

            GameManager.Instance.AllInstances.Add(newEnemyInstance);
        }
    }
}