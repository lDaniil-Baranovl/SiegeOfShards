//using UnityEngine;
//using UnityEngine.UI;

//public class AIDebugUI : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private SmartAIOpponent aiOpponent;
//    [SerializeField] private BattlefieldAnalyzer analyzer;

//    [Header("UI Elements")]
//    [SerializeField] private Text debugText;
//    [SerializeField] private bool showDebug = true;

//    [Header("UI Position")]
//    [SerializeField] private bool topLeft = true;

//    private void Start()
//    {
//        if (debugText == null)
//        {
//            CreateDebugUI();
//        }
//    }

//    private void CreateDebugUI()
//    {
//        GameObject canvas = GameObject.Find("Canvas");
//        if (canvas == null)
//        {
//            canvas = new GameObject("DebugCanvas");
//            Canvas c = canvas.AddComponent<Canvas>();
//            c.renderMode = RenderMode.ScreenSpaceOverlay;
//            canvas.AddComponent<CanvasScaler>();
//            canvas.AddComponent<GraphicRaycaster>();
//        }

//        GameObject textObj = new GameObject("AIDebugText");
//        textObj.transform.SetParent(canvas.transform, false);

//        debugText = textObj.AddComponent<Text>();
//        debugText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
//        debugText.fontSize = 14;
//        debugText.color = Color.white;
//        debugText.alignment = TextAnchor.UpperLeft;

//        RectTransform rect = textObj.GetComponent<RectTransform>();
//        rect.anchorMin = topLeft ? new Vector2(0, 1) : new Vector2(1, 1);
//        rect.anchorMax = topLeft ? new Vector2(0, 1) : new Vector2(1, 1);
//        rect.pivot = topLeft ? new Vector2(0, 1) : new Vector2(1, 1);
//        rect.anchoredPosition = topLeft ? new Vector2(10, -10) : new Vector2(-10, -10);
//        rect.sizeDelta = new Vector2(300, 200);

//        GameObject bgObj = new GameObject("Background");
//        bgObj.transform.SetParent(textObj.transform, false);
//        bgObj.transform.SetAsFirstSibling();

//        Image bg = bgObj.AddComponent<Image>();
//        bg.color = new Color(0, 0, 0, 0.7f);

//        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
//        bgRect.anchorMin = Vector2.zero;
//        bgRect.anchorMax = Vector2.one;
//        bgRect.sizeDelta = Vector2.zero;
//    }

//    private void Update()
//    {
//        if (!showDebug || debugText == null || aiOpponent == null || analyzer == null)
//            return;

//        BattlefieldAnalyzer.BattlefieldState state = analyzer.AnalyzeBattlefield();

//        string debugInfo = "<b>=== AI DEBUG ===</b>\n";
//        debugInfo += $"<color=yellow>Elixir:</color> {aiOpponent.GetCurrentElixir()}/10\n";
//        debugInfo += $"<color=cyan>Strategy:</color> {aiOpponent.GetCurrentStrategy()}\n";
//        debugInfo += $"<color=red>Threat:</color> {state.threatLevel}\n";

//        debugInfo += $"\n<b>Hand ({aiOpponent.GetCurrentHand().Count}/{aiOpponent.GetHandSize()}):</b>\n";
//        foreach (var card in aiOpponent.GetCurrentHand())
//        {
//            if (card != null)
//            {
//                string cardColor = card.elixirCost <= aiOpponent.GetCurrentElixir() ? "lime" : "gray";
//                debugInfo += $"  <color={cardColor}>{card.unitName}({card.elixirCost})</color>\n";
//            }
//        }

//        debugInfo += $"\n<b>Units:</b>\n";
//        debugInfo += $"  Player: {state.playerUnitsCount}\n";
//        debugInfo += $"  - Flying: <color=cyan>{state.playerFlyingCount}</color>\n";
//        debugInfo += $"  - Ground: {state.playerGroundCount}\n";
//        debugInfo += $"  AI: {state.aiUnitsCount}\n";
//        debugInfo += $"  Near Tower: {state.playerUnitsNearTower}\n";
//        debugInfo += $"\n<b>Player State:</b>\n";
//        debugInfo += $"  Attacking: {(state.playerIsAttacking ? "YES" : "NO")}\n";
//        debugInfo += $"  Flying Threat: {(state.hasFlyingThreat ? "<color=red>YES</color>" : "NO")}\n";
//        debugInfo += $"  Passive: {(state.playerIsPassive ? "YES" : "NO")}\n";
//        debugInfo += $"  Idle Time: {state.timeSinceLastPlayerAction:F1}s\n";

//        debugText.text = debugInfo;
//    }

//    public void ToggleDebug()
//    {
//        showDebug = !showDebug;
//        if (debugText != null)
//        {
//            debugText.gameObject.SetActive(showDebug);
//        }
//    }
//}
