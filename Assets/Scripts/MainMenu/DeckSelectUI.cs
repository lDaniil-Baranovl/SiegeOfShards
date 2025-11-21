using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DeckSelectUI : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("Все доступные карточки (UnitCost ScriptableObjects)")]
    public UnitCost[] allCards;

    [Header("UI Prefabs & Parents")]
    public GameObject cardButtonPrefab;      // префаб кнопки карточки в магазине (Image + Button + icon + cost text)
    public Transform shopContentParent;      // куда инстансим все карточки (scroll/content)
    public Transform deckSlotsParent;        // где 8 слотов (UI пустые ячейки)

    [Header("UI References")]
    public Button playButton;
    public Text deckCountText;

    private List<UnitCost> deck = new List<UnitCost>(8);
    private List<GameObject> shopButtons = new List<GameObject>();
    private GameObject[] deckSlotInstances; // визуальные объекты в слотах

    private const int MaxDeckSize = 8;

    void Start()
    {
        // Инициализация слотов (предполагается, что deckSlotsParent содержит ровно MaxDeckSize пустых объектов)
        deckSlotInstances = new GameObject[MaxDeckSize];
        for (int i = 0; i < MaxDeckSize; i++)
        {
            if (i < deckSlotsParent.childCount)
                deckSlotInstances[i] = deckSlotsParent.GetChild(i).gameObject;
            else
                deckSlotInstances[i] = null;
        }

        PopulateShop();
        UpdateDeckUI();
        playButton.onClick.AddListener(OnPlayPressed);
    }

    private void PopulateShop()
    {
        // Очищаем старые кнопки (если есть)
        foreach (Transform t in shopContentParent) Destroy(t.gameObject);
        shopButtons.Clear();

        // Создаём кнопку для каждой карты
        for (int i = 0; i < allCards.Length; i++)
        {
            UnitCost data = allCards[i];
            GameObject go = Instantiate(cardButtonPrefab, shopContentParent);
            shopButtons.Add(go);

            // Допустим в prefab есть компоненты:
            // - Button (root)
            // - Image icon (child named "Icon")
            // - Text costText (child named "CostText")
            // - Text nameText (child named "NameText")
            Button btn = go.GetComponent<Button>();
            Image icon = go.transform.Find("Icon")?.GetComponent<Image>();
            Text costText = go.transform.Find("CostText")?.GetComponent<Text>();
            Text nameText = go.transform.Find("NameText")?.GetComponent<Text>();

            if (icon != null && data != null)
            {
                // Если UnitCost содержит Sprite или иконку - присвой. Иначе оставь пустым.
                // Допустим UnitCost имеет поле Sprite icon (если нет — добавь).
                var icoField = data.GetType().GetField("icon");
                if (icoField != null)
                {
                    Sprite sp = icoField.GetValue(data) as Sprite;
                    if (sp != null) icon.sprite = sp;
                }
            }

            if (costText != null)
                costText.text = data.elixirCost.ToString();

            if (nameText != null)
                nameText.text = data.unitName;

            int index = i; // замыкание
            btn.onClick.AddListener(() => OnShopCardClicked(index));
        }
    }

    private void OnShopCardClicked(int shopIndex)
    {
        // Добавить в колоду, если есть место
        if (deck.Count >= MaxDeckSize)
        {
            // Можно добавить всплывающее "Колода полна"
            Debug.Log("Deck full");
            return;
        }

        UnitCost toAdd = allCards[shopIndex];
        deck.Add(toAdd);
        UpdateDeckUI();
    }

    // Позволяет удалить карту из слота по индексу (если захотим)
    public void RemoveCardFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= deck.Count) return;

        deck.RemoveAt(slotIndex);
        UpdateDeckUI();
    }

    private void UpdateDeckUI()
    {
        // Обновляем текст количества
        deckCountText.text = $"{deck.Count} / {MaxDeckSize}";

        // Обновляем слоты: показываем картинки и стоимость, либо пустой слот
        for (int i = 0; i < MaxDeckSize; i++)
        {
            Transform slot = deckSlotsParent.GetChild(i);
            Image icon = slot.Find("Icon")?.GetComponent<Image>();
            Text costText = slot.Find("CostText")?.GetComponent<Text>();
            Button slotBtn = slot.GetComponent<Button>();

            if (i < deck.Count)
            {
                UnitCost d = deck[i];
                if (icon != null && d != null)
                {
                    var icoField = d.GetType().GetField("icon");
                    if (icoField != null)
                    {
                        Sprite sp = icoField.GetValue(d) as Sprite;
                        if (sp != null) icon.sprite = sp;
                    }
                }

                if (costText != null) costText.text = d.elixirCost.ToString();

                // показываем слот активным
                slot.gameObject.SetActive(true);

                // назначаем удаление по клику (если пользователь хочет убрать выбранную карту)
                int si = i;
                if (slotBtn != null)
                {
                    slotBtn.onClick.RemoveAllListeners();
                    slotBtn.onClick.AddListener(() => RemoveCardFromSlot(si));
                }
            }
            else
            {
                // пустой слот
                if (icon != null) icon.sprite = null;
                if (costText != null) costText.text = "";
                // отключаем кликабельность
                if (slotBtn != null)
                {
                    slotBtn.onClick.RemoveAllListeners();
                }
            }
        }

        // Блокируем кнопку Play, если колода не полна
        playButton.interactable = (deck.Count == MaxDeckSize);
    }

    private void OnPlayPressed()
    {
        if (deck.Count != MaxDeckSize)
        {
            Debug.Log("Deck not full");
            return;
        }

        // Копируем в DeckManager и грузим сцену боя
        for (int i = 0; i < MaxDeckSize; i++)
            DeckManager.Instance.selectedDeck[i] = deck[i];

        // Здесь имя сцены боя (замени на своё)
        SceneManager.LoadScene("BattleScene");
    }
}
