using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq; // IEnumerable.Any() 사용을 위해 추가
[Flags]
public enum GridAreaType
{
    None,
    [TypeInfo(typeof(CardInstance))] Player = 1 << 0, // 플레이어 유닛 배치 영역 (외곽)
    [TypeInfo(typeof(EnemyInstance))] Enemy = 1 << 1      // 적 스폰 영역 (중앙)
}

[System.Serializable]
public struct GridAreaSetting
{
    [Tooltip("이 설정이 적용될 영역 타입")]
    public GridAreaType areaType;

    [Header("시각적 요소")]
    [Tooltip("그리드 셀의 기본 색상")]
    public Color areaColor;
    [Tooltip("그리드 셀에 사용될 기본 스프라이트 (선택 사항)")]
    public Sprite areaSprite;
}
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int totalGridWidth = 14;  // 전체 그리드 폭
    public int totalGridHeight = 14; // 전체 그리드 높이
    [Header("영역 설정")]
    [Tooltip("각 영역 타입에 대한 시각적 및 게임플레이 규칙 설정")]
    public List<GridAreaSetting> areaSettings = new List<GridAreaSetting>();
    public int enemyAreaOffset = 2;  // 외곽 배치 구역 너비 (중앙 10x10을 만들기 위한 오프셋)

    [Header("References")]
    public GameObject gridCellPrefab; // 각 칸을 나타내는 프리팹
    public Transform gridParent;      // 그리드 셀들을 담을 부모 오브젝트

    private GridCell[,] gridCells;    // 모든 그리드 셀 참조 배열

    private Dictionary<Type, GridAreaType> TypeToAreaFlags;

    // 싱글톤 패턴 (선택 사항이지만 매니저에 권장)
    public static GridManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (gridCellPrefab == null)
        {
            Debug.LogError("GridCell Prefab is not assigned to GridManager!");
            return;
        }

        if (gridParent == null)
        {
            gridParent = new GameObject("GridCells").transform;
            gridParent.SetParent(this.transform);
        }
        TypeToAreaFlags = new Dictionary<Type, GridAreaType>();

        // GridAreaType 열거형의 필드를 순회하며 리플렉션 실행 (단 한 번!)
        foreach (var field in typeof(GridAreaType).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.Name == nameof(GridAreaType.None)) continue;

            var typeInfo = field.GetCustomAttribute<TypeInfoAttribute>();

            if (typeInfo != null)
            {
                Type unitType = typeInfo.Type;
                GridAreaType currentFlag = (GridAreaType)field.GetValue(null);

                // 이미 맵핑된 타입이라면 기존 플래그에 누적 (OR 연산)
                if (TypeToAreaFlags.ContainsKey(unitType))
                {
                    TypeToAreaFlags[unitType] |= currentFlag;
                }
                // 새로운 타입이라면 딕셔너리에 추가
                else
                {
                    TypeToAreaFlags.Add(unitType, currentFlag);
                }
            }
        }

        GenerateGrid();
    }
    public GridAreaType GetTypeAreaFlags(Type unitType)
    {
        // unitType이 정확히 일치하는 경우를 먼저 조회
        if (TypeToAreaFlags.TryGetValue(unitType, out GridAreaType flags))
        {
            return flags;
        }

        GridAreaType allowedFlags = GridAreaType.None;

        foreach (var kvp in TypeToAreaFlags)
        {
            if (kvp.Key.IsAssignableFrom(unitType)) // unitType이 kvp.Key (CardInstance, EnemyInstance 등)에 할당 가능한가?
            {
                allowedFlags |= kvp.Value;
            }
        }

        // 한 번 찾은 결과를 캐시하여 다음부터는 바로 반환
        if (allowedFlags != GridAreaType.None)
        {
            TypeToAreaFlags.Add(unitType, allowedFlags);
        }

        return allowedFlags;
    }

    public GridCell GetGridCell(int x, int y)
    {
        if (x >= 0 && x < totalGridWidth && y >= 0 && y < totalGridHeight)
        {
            return gridCells[x, y];
        }
        return null;
    }

    // 중앙 10x10 영역의 모든 GridCell을 반환
    public List<GridCell> GetAllEnemySpawnCells()
    {
        List<GridCell> enemySpawnCells = new List<GridCell>();
        for (int y = enemyAreaOffset; y < totalGridHeight - enemyAreaOffset; y++)
        {
            for (int x = enemyAreaOffset; x < totalGridWidth - enemyAreaOffset; x++)
            {
                enemySpawnCells.Add(gridCells[x, y]);
            }
        }
        return enemySpawnCells;
    }

    // 외곽 배치 구역의 모든 GridCell을 반환
    public List<GridCell> GetAllPlayerPlacementCells()
    {
        List<GridCell> placementCells = new List<GridCell>();
        for (int y = 0; y < totalGridHeight; y++)
        {
            for (int x = 0; x < totalGridWidth; x++)
            {
                if (IsPlayerPlacementArea(x, y))
                {
                    placementCells.Add(gridCells[x, y]);
                }
            }
        }
        return placementCells;
    }


    void GenerateGrid()
    {
        gridCells = new GridCell[totalGridWidth, totalGridHeight];
        // 그리드 중앙에 정렬하기 위한 시작 위치 계산
        Vector3 startPos = -new Vector3(totalGridWidth / 2f - 0.5f, totalGridHeight / 2f - 0.5f, 0);

        for (int y = 0; y < totalGridHeight; y++)
        {
            for (int x = 0; x < totalGridWidth; x++)
            {
                Vector3 cellPos = startPos + new Vector3(x, y, 0);
                GameObject cellGO = Instantiate(gridCellPrefab, cellPos, Quaternion.identity);
                cellGO.transform.SetParent(gridParent);
                cellGO.name = $"GridCell_{x},{y}";

                GridCell cell = cellGO.GetComponent<GridCell>();
                if (cell != null)
                {
                    // 🌟 수정 1: 좌표(x, y)에 해당하는 GridAreaSetting을 결정
                    GridAreaType determinedType = DetermineAreaType(x, y);
                    GridAreaSetting setting = GetSettingForAreaType(determinedType);

                    // 🌟 수정 2: 수정된 Initialize 메서드 호출
                    cell.Initialize(x, y, setting);
                    gridCells[x, y] = cell;
                }
                else
                {
                    Debug.LogError($"GridCell prefab at {x},{y} does not have a GridCell component!");
                }
            }
        }
        Debug.Log($"Generated {totalGridWidth}x{totalGridHeight} grid.");
    }
    // 🌟 새로 추가되거나 수정된 헬퍼 메서드: 좌표에 따라 영역 타입을 결정
    private GridAreaType DetermineAreaType(int x, int y)
    {   
        // 중앙 영역 (적 스폰 영역)
        if (x >= enemyAreaOffset && x < totalGridWidth - enemyAreaOffset &&
            y >= enemyAreaOffset && y < totalGridHeight - enemyAreaOffset)
        {
            return GridAreaType.Enemy;
        }
        // 외곽 영역 (플레이어 배치 영역)
        else
        {
            return GridAreaType.Player;
        }
        // 참고: IsEnemySpawnArea, IsPlayerPlacementArea 메서드는 이제 DetermineAreaType이 대체합니다.
    }

    // 🌟 새로 추가된 헬퍼 메서드: 영역 타입에 해당하는 GridAreaSetting을 찾음
    private GridAreaSetting GetSettingForAreaType(GridAreaType areaType)
    {
        // areaSettings 리스트를 순회하며 일치하는 설정을 찾습니다.
        foreach (var setting in areaSettings)
        {
            if (setting.areaType == areaType)
            {
                return setting;
            }
        }
        // 일치하는 설정이 없을 경우 오류 로그 후 기본값 반환 (잠재적 문제점 3)
        Debug.LogError($"GridAreaSetting for {areaType} not found! Returning default.");
        return default; // GridAreaSetting의 기본값 반환
    }

    private bool IsEnemySpawnArea(int x, int y)
    {
        return x >= enemyAreaOffset && x < totalGridWidth - enemyAreaOffset &&
               y >= enemyAreaOffset && y < totalGridHeight - enemyAreaOffset;
    }

    private bool IsPlayerPlacementArea(int x, int y)
    {
        return !IsEnemySpawnArea(x, y);
    }
}