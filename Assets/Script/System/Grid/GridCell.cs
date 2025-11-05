using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq; // IEnumerable.Any() 사용을 위해 추가

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class GridCell : MonoBehaviour
{
    public int x;
    public int y;
    public GridAreaSetting CurrentArea { get; private set; }

    // 현재 셀이 비어있는지 빠르게 확인하는 속성
    public bool IsOccupied => currentBaseInstance != null;

    public BaseInstance currentBaseInstance;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // **Initialize 메서드 수정: GridAreaSetting을 인수로 받음**
    public void Initialize(int x, int y, GridAreaSetting setting)
    {
        this.x = x;
        this.y = y;
        this.CurrentArea = setting;
        // **시각적 속성 설정**
        spriteRenderer.color = setting.areaColor;
        if (setting.areaSprite != null)
        {
            spriteRenderer.sprite = setting.areaSprite;
        }
    }
    public bool CanPlaceInstance(BaseInstance unitInstance)
    {
        // 1. 셀이 이미 점유되어 있으면 배치 불가
        if (IsOccupied)
        {
            return false;
        }

        // 2. 배치하려는 유닛의 타입에 해당하는 허용된 영역 플래그를 레지스트리에서 조회
        Type unitType = unitInstance.GetType();

        // 🚨 수정된 부분: 리플렉션 대신 중앙 레지스트리 조회
        GridAreaType allowedFlags = GridManager.Instance.GetTypeAreaFlags(unitType);
        
        // 4. 셀의 영역 타입이 유닛이 요구하는 플래그 중 하나라도 포함하는지 최종 확인
        if ((CurrentArea.areaType & allowedFlags) != GridAreaType.None)
        {
            // 🚨 사용자 지시에 따라 배치 실행 코드를 여기에 추가함 (SRP 위반)
            currentBaseInstance = unitInstance;
            return true;
        }

        return false;
    }


    // Highlight 메서드 수정
    public void Highlight(Color highlightColor)
    {
        // 강조 시에는 originalColor와 무관하게 즉시 색상 변경
        spriteRenderer.color = highlightColor;
    }

    // 유닛을 셀에 배치/제거하는 유틸리티 메서드
    public void SetInstance(BaseInstance instance)
    {
        currentBaseInstance = instance;
    }

    public void RemoveInstance()
    {
        currentBaseInstance = null;
    }
    // OnMouseDown 메서드 예시
    void OnMouseDown()
    {
        Debug.Log($"Clicked ({x},{y})");
        // 여기서 allowsPlayerUnit을 확인하여 유닛 배치 가능 여부를 판단할 수 있습니다.
    }
}