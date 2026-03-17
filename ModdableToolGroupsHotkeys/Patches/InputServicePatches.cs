namespace ModdableToolGroupsHotkeys.Patches;

/// <summary>
/// Patches InputService methods to prevent consumed keys from triggering other actions.
/// When a hotkey combination is active (e.g., Shift+S for stairs), the individual keys
/// (S) should not trigger other actions like camera movement.
/// </summary>
[HarmonyPatch(typeof(InputService))]
public static class InputServicePatches
{
    /// <summary>
    /// Intercept IsKeyDown to block consumed keys.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(InputService.IsKeyDown))]
    public static bool IsKeyDown_Prefix(string keyBindingId, ref bool __result)
    {
        var tracker = ConsumedKeyTracker.Instance;
        if (tracker == null) return true;

        if (tracker.IsKeyPathConsumed(keyBindingId))
        {
            __result = false;
            return false; // Skip original method
        }

        return true; // Run original method
    }

    /// <summary>
    /// Intercept IsKeyHeld to block consumed keys.
    /// This is the key fix for the camera movement issue.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(InputService.IsKeyHeld))]
    public static bool IsKeyHeld_Prefix(string keyBindingId, ref bool __result)
    {
        var tracker = ConsumedKeyTracker.Instance;
        if (tracker == null) return true;

        if (tracker.IsKeyPathConsumed(keyBindingId))
        {
            __result = false;
            return false; // Skip original method
        }

        return true; // Run original method
    }

    /// <summary>
    /// Intercept IsKeyPressed to block consumed keys.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(InputService.IsKeyPressed))]
    public static bool IsKeyPressed_Prefix(string keyBindingId, ref bool __result)
    {
        var tracker = ConsumedKeyTracker.Instance;
        if (tracker == null) return true;

        if (tracker.IsKeyPathConsumed(keyBindingId))
        {
            __result = false;
            return false; // Skip original method
        }

        return true; // Run original method
    }
}
