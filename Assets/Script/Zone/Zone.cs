using System.Collections.Generic;
using UnityEngine;

// 모든 "자리"의 공통 컨테이너. ItemKind/IZone 같은 건 없음.
// BaseInstance만 담는다(카드/유물/소모품이 BaseInstance를 상속한다고 가정).
public class Zone
{
    public string Name { get; private set; }

    // 내부 저장소
    protected readonly List<BaseInstance> _items = new List<BaseInstance>();
    public IReadOnlyList<BaseInstance> Items => _items;

    public Zone(string name)
    {
        Name = name;
    }

    // 가벼운 기본 동작 (특수 처리는 파생 클래스에서 오버라이드)
    public virtual void Add(BaseInstance inst)
    {
        if (inst == null) return;
        if (_items.Contains(inst)) return;
        _items.Add(inst);
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
