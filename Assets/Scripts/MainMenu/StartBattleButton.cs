using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartBattleButton : MonoBehaviour
{
    public Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(OnStart);
    }

    private void OnStart()
    {
        if (DeckManager.Instance.selectedDeck.Count == 8)
        {
            SceneManager.LoadScene("testMechanic");
        }
        else
        {
            Debug.Log("Нужно выбрать ровно 8 карт.");
        }
    }
}
