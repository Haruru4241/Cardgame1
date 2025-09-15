using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[CreateAssetMenu(menuName = "CardGame/ArtifactData")]
public class ArtifactData : BaseData
{
    [Header("유물 발동 이벤트")]
    public List<BaseAction> onDrawActions = new List<BaseAction>();
    public List<BaseAction> onUseActions = new List<BaseAction>();
    // 필요 시 다른 시그널도 추가
}
