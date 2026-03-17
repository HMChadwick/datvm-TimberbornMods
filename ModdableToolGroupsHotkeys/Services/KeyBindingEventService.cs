namespace ModdableToolGroupsHotkeys.Services;

public class KeyBindingEventService(
    InputService inputService,
    ConsumedKeyTracker consumedKeyTracker
) : IInputProcessor, ILoadableSingleton
{
    readonly Dictionary<string, KeyBindingEvent> mapper = [];

    public KeyBindingEvent Get(string id) => mapper.GetOrAdd(id, () => new(id));

    public void Load()
    {
        inputService.AddInputProcessor(this);
    }

    public bool ProcessInput()
    {
        // Clear consumed keys from the previous frame
        consumedKeyTracker.ClearConsumedKeys();

        // Check for hotkey activations
        foreach (var ev in mapper.Values)
        {
            if (inputService.IsKeyDown(ev.KeyBindingId))
            {
                // Mark this key binding as consumed
                consumedKeyTracker.ConsumeKeyBinding(ev.KeyBindingId);

                ev.RaiseOnDown();
                return true;
            }
        }

        return false;
    }
}

public class KeyBindingEvent(string id)
{

    public readonly string KeyBindingId = id;

    public event Action? OnDown;
    internal void RaiseOnDown() => OnDown?.Invoke();

}