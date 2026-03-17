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
    /// Check if a key binding ID's keys are currently consumed by active hotkeys.
    /// </summary>
    public bool IsKeyBindingConsumed(string keyBindingId)
    {
        // Get the keys used by this binding
        var targetBinding = keyBindingRegistry.Get(keyBindingId);
        var targetKeys = GetKeysFromBinding(targetBinding);

        if (targetKeys.Count == 0) return false;

        // Check if any consumed hotkey uses overlapping keys
        foreach (var consumedBindingId in consumedKeyBindingIds)
        {
            var consumedBinding = keyBindingRegistry.Get(consumedBindingId);
            var consumedKeys = GetKeysFromBinding(consumedBinding);

            // If the consumed binding uses any of the same keys, block it
            if (targetKeys.Overlaps(consumedKeys))
            {
                return true;
            }
        }

        return false;
    }

    HashSet<string> GetKeysFromBinding(KeyBinding binding)
    {
        var keys = new HashSet<string>();

        // Get keys from both primary and secondary bindings
        AddKeysFromInputBinding(keys, binding.PrimaryInputBinding);
        AddKeysFromInputBinding(keys, binding.SecondaryInputBinding);

        return keys;
    }

    void AddKeysFromInputBinding(HashSet<string> keys, InputBinding? binding)
    {
        if (binding == null) return;

        // Extract the key from the path
        var key = ExtractKeyFromPath(binding.Path);
        if (key != null)
        {
            keys.Add(NormalizeKeyPath(key));
        }

        // TODO: Also extract modifier keys if we need more precise matching
        // For now, just the main key is sufficient
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
