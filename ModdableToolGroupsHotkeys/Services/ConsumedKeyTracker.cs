namespace ModdableToolGroupsHotkeys.Services;

/// <summary>
/// Tracks which key bindings are currently consumed by active hotkeys,
/// preventing them from triggering other actions (like camera movement).
/// </summary>
public class ConsumedKeyTracker : ILoadableSingleton
{
    public static ConsumedKeyTracker? Instance { get; private set; }

    readonly HashSet<string> consumedKeyBindingIds = [];
    readonly KeyBindingRegistry keyBindingRegistry;

    public ConsumedKeyTracker(KeyBindingRegistry keyBindingRegistry)
    {
        this.keyBindingRegistry = keyBindingRegistry;
    }

    public void Load()
    {
        Instance = this;
    }

    /// <summary>
    /// Mark a key binding as consumed for the current frame.
    /// Should be called when a hotkey is triggered.
    /// </summary>
    public void ConsumeKeyBinding(string keyBindingId)
    {
        consumedKeyBindingIds.Add(keyBindingId);
    }

    /// <summary>
    /// Check if a key path (e.g., "S", "Shift+S") is currently consumed.
    /// </summary>
    public bool IsKeyPathConsumed(string keyPath)
    {
        // Check if any consumed key binding uses this key path
        foreach (var bindingId in consumedKeyBindingIds)
        {
            var binding = keyBindingRegistry.Get(bindingId);

            // Check both primary and secondary bindings
            if (IsKeyPathInBinding(keyPath, binding.PrimaryInputBinding) ||
                IsKeyPathInBinding(keyPath, binding.SecondaryInputBinding))
            {
                return true;
            }
        }

        return false;
    }

    bool IsKeyPathInBinding(string keyPath, InputBinding? binding)
    {
        if (binding == null) return false;

        // The binding's Path contains the key (e.g., "<Keyboard>/s")
        // We need to check if this matches the requested key path
        // The keyPath might be just "S" or with modifiers

        // Extract the key from the binding path
        var bindingKey = ExtractKeyFromPath(binding.Path);
        if (bindingKey == null) return false;

        // Normalize both for comparison
        var normalizedKeyPath = NormalizeKeyPath(keyPath);
        var normalizedBindingPath = NormalizeKeyPath(bindingKey);

        return normalizedBindingPath == normalizedKeyPath;
    }

    string? ExtractKeyFromPath(string path)
    {
        // Input paths look like "<Keyboard>/s" or "<Mouse>/leftButton"
        // Extract just the key part after the last /
        var lastSlash = path.LastIndexOf('/');
        if (lastSlash < 0) return null;

        return path.Substring(lastSlash + 1);
    }

    string NormalizeKeyPath(string keyPath)
    {
        // Remove device prefix if present
        var normalized = keyPath.ToLowerInvariant();
        if (normalized.StartsWith("<keyboard>/"))
        {
            normalized = normalized.Substring("<keyboard>/".Length);
        }

        return normalized;
    }

    /// <summary>
    /// Clear consumed keys at the end of each frame.
    /// Should be called by the input processor after all input is processed.
    /// </summary>
    public void ClearConsumedKeys()
    {
        consumedKeyBindingIds.Clear();
    }
}
