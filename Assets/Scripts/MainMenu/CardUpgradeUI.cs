using TMPro;
using UnityEngine;

public class CardUpgradeUI : MonoBehaviour
{
    public UnitCost unit;
    public int upgradeCost = 100;

    public TextMeshProUGUI textUI;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        var data = CardUpgradeManager.Instance.GetCard(unit);
        textUI.text = $"{data.level}ур {data.fragments}/10";
    }
    public void TryUpgrade()
    {
        bool ok = CardUpgradeManager.Instance.TryUpgrade(unit, upgradeCost);

        if (!ok)
        {
            Debug.Log("Недостаточно карт или золота!");
        }
        else
        {
            Debug.Log($"Карта {unit.unitName} улучшена!");
        }

        UpdateUI();
    }
}
