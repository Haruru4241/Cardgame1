using System.Collections.Generic;

// 카드의 "자리"를 표현하는 Zone 파생형.
// Deck/Hand/Discard... 등은 PileType으로만 구분한다(이름 하드코딩 X).
public class Pile : Zone
{
    public PileType Type { get; private set; }

    // 기존 코드 호환을 위해 Cards 프로퍼티 제공(내부 리스트 그대로 노출)
    public List<BaseInstance> Cards => _items;

    public Pile(PileType type) : base(type.ToString())
    {
        Type = type;
        // Name은 type.ToString()으로 자동 설정(원하면 로컬라이즈/표기 바꿔도 됨)
    }

    public override void Add(BaseInstance ci)
    {
        if (ci == null) return;
        if (_items.Contains(ci)) return;

        _items.Add(ci);

        // 기존 호환: 카드 위치 기록
        // (CardInstance가 BaseInstance 상속 & CurrentPile 보유 가정)
        if (ci.CurrentZone != this)
            ci.CurrentZone = this;
    }

    public override bool Remove(BaseInstance ci)
    {
        if (ci == null) return false;

        if (_items.Remove(ci))
        {
            if (ci.CurrentZone == this)
                ci.CurrentZone = null;
            return true;
        }
        return false;
    }

    // 필요 시 Pile 전용 편의 함수 추가 가능
}
