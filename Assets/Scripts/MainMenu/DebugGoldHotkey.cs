using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

#if UNITY_EDITOR
public class DebugGoldHotkey : MonoBehaviour
{
    [Tooltip("Сколько золота начислять по нажатию клавиши G")]
    public int goldAmount = 1000;

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        bool pressed = Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame;
#else
        bool pressed = Input.GetKeyDown(KeyCode.G);
#endif

        if (pressed)
        {
            GoldManager.Instance.AddGold(goldAmount);
            Debug.Log($"[DEBUG] +{goldAmount} золота. Баланс: {GoldManager.Instance.Gold}");
        }
    }
}
#endif
