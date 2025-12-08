using TMPro;
using UnityEngine;

public class TryOpenCaseManager : MonoBehaviour
{
    public static TryOpenCaseManager Instance;

    [Header("UI")]
    public TextMeshProUGUI caseAttemptsText;

    public int CaseAttempts { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CaseAttempts = PlayerPrefs.GetInt("CaseAttempts", 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (caseAttemptsText != null)
            caseAttemptsText.text = CaseAttempts.ToString();
    }

    public void AddCaseAttempt(int amount)
    {
        CaseAttempts += amount;
        PlayerPrefs.SetInt("CaseAttempts", CaseAttempts);
        PlayerPrefs.Save();
        UpdateUI();
    }

    // Метод вызывается кнопкой "Открыть кейс"
    public bool UseCaseAttempt()
    {
        if (CaseAttempts <= 0)
            return false;

        CaseAttempts--;
        PlayerPrefs.SetInt("CaseAttempts", CaseAttempts);
        PlayerPrefs.Save();
        UpdateUI();

        return true;
    }

    // Метод покупки кейса за золото
    public bool BuyCase(int price)
    {
        if (GoldManager.Instance.Gold < price)
            return false;

        GoldManager.Instance.AddGold(-price);
        return true;
    }
}
