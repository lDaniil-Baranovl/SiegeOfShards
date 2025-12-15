using System;
using UnityEngine;

/// <summary>
/// Глобальные события для системы размещения арены
/// Используется для уведомления других систем (например, ИИ) о размещении арены
/// </summary>
public static class ArenaPlacementEvents
{
    /// <summary>
    /// Событие вызывается когда арена была подтверждена и размещена игроком
    /// </summary>
    public static event Action OnArenaConfirmed;

    /// <summary>
    /// Флаг, указывающий размещена ли арена
    /// </summary>
    public static bool IsArenaPlaced { get; private set; } = false;

    /// <summary>
    /// Вызвать событие подтверждения арены
    /// </summary>
    public static void InvokeArenaConfirmed()
    {
        IsArenaPlaced = true;
        OnArenaConfirmed?.Invoke();
        Debug.Log("[ArenaPlacementEvents] Arena confirmed and placed!");
    }

    /// <summary>
    /// Сбросить состояние (для тестирования)
    /// </summary>
    public static void Reset()
    {
        IsArenaPlaced = false;
        Debug.Log("[ArenaPlacementEvents] Arena state reset");
    }
}
