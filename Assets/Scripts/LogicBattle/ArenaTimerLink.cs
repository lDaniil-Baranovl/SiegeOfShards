using TMPro;
using UnityEngine;

// Висит на Timer-объекте внутри префаба локации.
// При появлении локации сам находит BattleManager и передаёт ему свой текст таймера.
public class ArenaTimerLink : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.SetTimerText(timerText);
        else
            Debug.LogWarning("[ArenaTimerLink] BattleManager.Instance not found");
    }
}
