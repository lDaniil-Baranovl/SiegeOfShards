using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    public int Gold { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ��������� ������ �� PlayerPrefs
            Gold = PlayerPrefs.GetInt("Gold", 150);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;

        // ���������
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.Save();
    }
}
