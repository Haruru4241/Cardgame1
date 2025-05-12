using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public int currentMana = 0;
    public int maxManaPerTurn = 2;
    public int money = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void StartTurn()
    {
        currentMana = maxManaPerTurn;
        Debug.Log($"턴 시작: 마나 {currentMana}");
    }

    public bool SpendMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            return true;
        }
        return false;
    }
    // *** 머니 관련 함수 추가 ***
    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"돈 증가: +{amount}, 현재 돈: {money}");
    }

    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"돈 사용: -{amount}, 현재 돈: {money}");
            return true;
        }
        Debug.Log($"돈 부족: {amount} 필요, 현재 돈: {money}");
        return false;
    }

    public void SetMoney(int value)
    {
        money = value;
        Debug.Log($"돈 값 설정: {money}");
    }
}
