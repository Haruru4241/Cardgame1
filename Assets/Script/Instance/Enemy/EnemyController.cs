using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// '적' 개체를 제어하는 컨트롤러입니다.
/// 적의 UI를 업데이트하고 플레이어와의 상호작용을 처리합니다.
/// </summary>
public class EnemyController : BaseController
{
    [Header("Enemy-Specific UI")]
    public TextMeshProUGUI healthText; // 적의 체력을 표시할 UI
    public TextMeshProUGUI DamageText; // 적의 체력을 표시할 UI

    /// <summary>
    /// 적의 상태가 변경될 때 호출되어 화면을 최신 상태로 갱신합니다.
    /// </summary>
    public override void UpdateUI()
    {
        var busesToPush = new List<SignalBus>();
        busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.NameEvaluation)));
        busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.DescriptionEvaluation)));
        busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.HPEvaluation)));
        busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.DealDamageEvaluation)));
        ReactionStackManager.Instance.PushBuses(busesToPush);
        nameText.text = (string)busesToPush[0].CalcRaw;
        descriptionText.text = (string)busesToPush[1].CalcRaw;
        healthText.text = ((int)busesToPush[2].CalcRaw).ToString();
        DamageText.text = ((int)busesToPush[3].CalcRaw).ToString();
    }
    public override void Setup(BaseData data, BaseInstance instance)
    {
        baseData = data;
        baseInstance = instance;
        UpdateUI();
    }
    public override void Use()
    {
        baseInstance.Fire(new SignalBus(SignalType.OnUse));
        baseInstance.Fire(new SignalBus(SignalType.OnEffect));
        baseInstance.Fire(new SignalBus(SignalType.OnPlayed));
    }
}