using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("Настройки боя")]
    public float matchDuration = 180f; // 3 минуты
    private float timer;

    [Header("UI Конец боя")]
    public GameObject endBattleCanvas;

    [Header("Тексты результата")]
    public GameObject victoryText;
    public GameObject defeatText;
    public GameObject drawText;

    private int blueDestroyed = 0;
    private int redDestroyed = 0;

    private bool battleEnded = false;
    public TextMeshProUGUI timerText;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        timer = matchDuration;
        endBattleCanvas.SetActive(false);

        // всё выключаем
        victoryText.SetActive(false);
        defeatText.SetActive(false);
        drawText.SetActive(false);
    }

    private void Update()
    {
        if (battleEnded) return;

        timer -= Time.deltaTime;

        UpdateTimerUI();

        if (timer <= 0)
        {
            EndBattleByTime();
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;

        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    // Башня сообщает о разрушении
    public void TowerDestroyed(int teamID)
    {
        if (battleEnded) return;

        if (teamID == 0)
            blueDestroyed++;
        else
            redDestroyed++;

        CheckInstantWin();
    }

    private void CheckInstantWin()
    {
        if (blueDestroyed >= 3)
        {
            ShowResult("lose"); // все синие башни уничтожены синие проиграли
        }
        else if (redDestroyed >= 3)
        {
            ShowResult("win"); // все красные башни уничтожены синие победили
        }
    }

    private void EndBattleByTime()
    {
        if (battleEnded) return;

        int blueAlive = 3 - blueDestroyed;
        int redAlive = 3 - redDestroyed;

        if (blueAlive > redAlive)
            ShowResult("win");
        else if (redAlive > blueAlive)
            ShowResult("lose");
        else
            ShowResult("draw");
    }

    private void ShowResult(string result)
    {
        battleEnded = true;

        endBattleCanvas.SetActive(true);

        // выключаем все
        victoryText.SetActive(false);
        defeatText.SetActive(false);
        drawText.SetActive(false);

        // включаем нужное
        if (result == "win")
            victoryText.SetActive(true);
        else if (result == "lose")
            defeatText.SetActive(true);
        else
            drawText.SetActive(true);

        if (result == "win")
            GoldManager.Instance.AddGold(40);
        else if (result == "draw")
            GoldManager.Instance.AddGold(20);
        else
            GoldManager.Instance.AddGold(10);

        // ставим игру на паузу
        GamePause.paused = true;
        Time.timeScale = 0f;
    }

    public void EndBattle()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
