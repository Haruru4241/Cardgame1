using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// '카드' 개체를 전문적으로 제어하는 컨트롤러입니다.
/// </summary>
public class CardController : BaseController
{
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI buyCostText;
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

    /// <summary>
    /// 카드의 상태 변경 시 호출되어 화면을 최신 상태로 갱신합니다.
    /// </summary>
    public override void UpdateUI()
    {
        if (this.baseInstance == null) return;

                var busesToPush = new List<SignalBus>();
        busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.NameEvaluation)));
        busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.DescriptionEvaluation)));
        busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.ManaCostEvaluation)));
        busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.BuyCostEvaluation)));
        ReactionStackManager.Instance.PushBuses(busesToPush);
        nameText.text = CalculationManager.Instance.Evaluate<string>(busesToPush[0], CalcType.Name);
        descriptionText.text = CalculationManager.Instance.Evaluate<string>(busesToPush[1], CalcType.Description);
        manaCostText.text = CalculationManager.Instance.Evaluate<string>(busesToPush[2], CalcType.ManaCost);
        buyCostText.text = CalculationManager.Instance.Evaluate<string>(busesToPush[3], CalcType.Cost);

        // --- 핵심 로직 (단 4줄) ---
        // var busesToPush = new List<SignalBus>();
        // busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.NameEvaluation)));
        // busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.DescriptionEvaluation)));
        // busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.ManaCostEvaluation)));
        // busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.BuyCostEvaluation)));
        // ReactionStackManager.Instance.PushBuses(busesToPush);
        // nameText.text = GetValue<string>(SignalType.NameEvaluation, CalcType.Name);
        // descriptionText.text = GetValue<string>(SignalType.DescriptionEvaluation, CalcType.Description);
        // manaCostText.text = GetValue<int>(SignalType.ManaCostEvaluation, CalcType.ManaCost).ToString();
        // buyCostText.text = GetValue<int>(SignalType.BuyCostEvaluation, CalcType.Cost).ToString();

        // var busesToPush = new List<SignalBus>();
        // busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.NameEvaluation)));
        // busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.DescriptionEvaluation)));
        // busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.ManaCostEvaluation)));
        // busesToPush.Add(this.baseInstance.PrepareBus(new SignalBus(SignalType.BuyCostEvaluation)));
        // ReactionStackManager.Instance.PushBuses(busesToPush);
        // nameText.text = (string)busesToPush[0].CalcRaw; 
        // descriptionText.text = (string)busesToPush[1].CalcRaw; 
        // manaCostText.text = ((int)busesToPush[2].CalcRaw).ToString(); 
        // buyCostText.text = ((int)busesToPush[3].CalcRaw).ToString(); 

        // // 5) 아트워크 (Sprite)
        // bus.Signal = SignalType.ArtworkEvaluation;
        // var artObj = cardInstance.Evaluate(bus);
        // if (bus.CalcKind == CellKind.Object && artObj is Sprite sp)
        //     artworkImage.sprite = sp;
        // else
        //     artworkImage.sprite = null; // 혹은 data.Artwork 같은 폴백

        // 7) 기타 UI 세팅
        if (backgroundImage != null)
            backgroundImage.color = Color.white;
    }
}