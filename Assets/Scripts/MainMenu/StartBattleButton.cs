using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartBattleButton : MonoBehaviour
{
    public GameObject[] buttonsEdit;
    private bool editMode = false;
    public void OnStart()
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
    public void OnEdit()
    {
        editMode = !editMode;
        foreach (var button in buttonsEdit) 
        {
            button.gameObject.SetActive(editMode);
        }
    }
}
