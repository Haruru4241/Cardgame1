using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 모든 "자리"의 공통 컨테이너. ItemKind/IZone 같은 건 없음.
// BaseInstance만 담는다(카드/유물/소모품이 BaseInstance를 상속한다고 가정).
public class Zone : IDropHandler
{
    public string Name { get; private set; }
    [Header("Zone 설정")]
    [Tooltip("이 영역의 타입입니다. (예: Hand, Shop, PlayArea)")]
    public ZoneType zoneType;
    // 내부 저장소
    protected readonly List<BaseInstance> _items = new List<BaseInstance>();
    public IReadOnlyList<BaseInstance> Items => _items;

    /// <summary>
    /// [2] Unity Event System이 'OnEndDrag'에서 호출합니다.
    /// 카드가 이 영역(Zone)에 '드롭'되었습니다.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        // 드래그 중이던 'BaseInstance'를 가져옵니다.
        if (eventData.pointerDrag != null)
        {
            BaseController card = eventData.pointerDrag.GetComponent<BaseController>();
            if (card != null)
            {
                // [!] 감지된 카드를 하위 클래스의 'HandleDrop' 로직으로 넘깁니다.
                HandleDrop(card);
            }
        }
    }

    /// <summary>
    /// [3] (하위 클래스가 구현)
    /// 이 Zone에 'card'가 드롭되었을 때 실행할 구체적인 로직입니다.
    /// </summary>
    protected virtual void HandleDrop(BaseController card)
    {
        // (기본 동작) 드롭을 허용하지 않고, 카드를 원래 부모에게 돌려보냄
        card.ReturnToOriginalParent();
    }
    public virtual bool CanDragFrom(BaseController card)
    {
        // (기본 동작) 드롭을 허용하지 않고, 카드를 원래 부모에게 돌려보냄
        return false;
    }

    // 가벼운 기본 동작 (특수 처리는 파생 클래스에서 오버라이드)
    public virtual void Add(BaseInstance inst)
    {
        if (inst == null) return;
        if (_items.Contains(inst)) return;
        _items.Add(inst);
        inst.CurrentZone = this;
    }

    public virtual bool Remove(BaseInstance inst)
    {
        if (inst == null) return false;
        return _items.Remove(inst);
    }

    public virtual List<BaseInstance> FindAll(System.Predicate<BaseInstance> predicate)
        => _items.FindAll(predicate);

    public virtual void Shuffle()
    {
        int n = _items.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_items[i], _items[j]) = (_items[j], _items[i]);
        }
    }

    // 필요시 이름 갱신 유틸
    protected void SetName(string name) => Name = name;
}
