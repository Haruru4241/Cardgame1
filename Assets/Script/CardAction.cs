using UnityEngine;
using System;

public abstract class CardAction : ScriptableObject
{
    // 즉시 실행
    public virtual void Execute(CardInstance card)
    {
        var func = GetFunction(null); // Processor 정보가 필요 없을 때는 null
        func?.Invoke(card);           // 카드 자체를 input으로 넘길 수도 있고 null도 가능
    }

    // 외부에서 등록 가능한 처리 함수
    public abstract Func<object, object> GetFunction(Processor processor);
}
