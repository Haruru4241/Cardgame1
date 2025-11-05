using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("플레이어 재화")]
    [SerializeField] private int currentGold; // 비용 (유동 자산)
    [SerializeField] private int currentMana;        // 마나값 (동력)
    public int maxManaPerTurn = 2;
    public int money = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // (필요시) DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartTurn()
    {
        currentMana = maxManaPerTurn;
        Debug.Log($"턴 시작: 마나 {currentMana}");
    }

    public void AddMana(int amount)
    {
        currentMana += amount;
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
    public bool TrySpendMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            return true;
        }
        return false; // 마나 부족
    }
    public void SetMana(int value)
    {
        currentMana = value;
    }
    // *** 머니 관련 함수 추가 ***
    public void AddGold(int amount)
    {
        money += amount;
        Debug.Log($"돈 증가: +{amount}, 현재 돈: {money}");
    }

    public bool SpendGold(int amount)
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
    public bool TrySpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            return true;
        }
        return false; // 비용 부족
    }

    public void SetGold(int value)
    {
        money = value;
        Debug.Log($"돈 값 설정: {money}");
    }
}
