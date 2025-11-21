using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    public UnitCost[] selectedDeck = new UnitCost[8];

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
