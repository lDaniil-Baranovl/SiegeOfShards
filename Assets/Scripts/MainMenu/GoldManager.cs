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

            // Загружаем золото из PlayerPrefs
            Gold = PlayerPrefs.GetInt("Gold", 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;

        // Сохраняем
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.Save();
    }
}
