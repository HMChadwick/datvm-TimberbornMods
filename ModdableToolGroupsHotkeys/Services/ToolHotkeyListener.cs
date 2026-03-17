namespace ModdableToolGroupsHotkeys.Services;

public class ToolHotkeyListener(
    ToolHotkeySpecService toolHotkeySpecService,
    KeyBindingEventService keyBindingEventService,
    ToolButtonService toolButtonService,
    ModdableToolGroupButtonService moddableToolGroupButtonService,
    CustomBlockObjectButtons customBlockObjectButtons
) : ILoadableSingleton
{

    public void Load()
    {
        foreach (var (id, btn) in toolHotkeySpecService.BlockObjectToolHotkeys)
        {
            RegisterAction(id, () => SelectToolButtonWithGroups(btn));
        }

        foreach (var (id, btn) in toolHotkeySpecService.BlockObjectGroupHotkeys)
        {
            RegisterAction(id, () => SelectGroupWithParents(btn.ToolGroupButton));
        }

        foreach (var (id, tool) in toolHotkeySpecService.ToolHotkeys)
        {
            RegisterAction(id, () => SelectToolWithGroup(tool));
        }
    }

    void SelectToolButtonWithGroups(ToolButton toolButton)
    {
        // Find which ModdableToolGroupButton contains this ToolButton
        var groupButton = FindModdableGroupForToolButton(toolButton);
        if (groupButton != null)
        {
            SelectGroupWithParents(groupButton.ToolGroupButton);
        }

        // Select the tool itself
        toolButton.Select();
    }

    void SelectToolWithGroup(IToolHotkeyDefinition toolHotkey)
    {
        // First, try to select the tool group that contains this tool
        if (toolHotkey is ButtonToolHotkeyDefinition btnHotkey)
        {
            var toolGroup = FindToolGroupForButton(btnHotkey.Button);
            if (toolGroup != null)
            {
                SelectGroupWithParents(toolGroup);
            }
        }

        // Then select the tool itself
        toolHotkey.Select();
    }

    void SelectGroupWithParents(ToolGroupButton groupButton)
    {
        // Get the parent chain
        var parentChain = GetParentChain(groupButton);

        // Select all parents from root to leaf
        foreach (var parent in parentChain)
        {
            parent.Select();
        }

        // Finally select the immediate group
        groupButton.Select();
    }

    List<ToolGroupButton> GetParentChain(ToolGroupButton groupButton)
    {
        var chain = new List<ToolGroupButton>();
        var info = moddableToolGroupButtonService[groupButton];

        while (info?.Parent != null)
        {
            chain.Insert(0, info.Parent.Button);
            info = info.Parent;
        }

        return chain;
    }

    ModdableToolGroupButton? FindModdableGroupForToolButton(ToolButton toolButton)
    {
        // Find which ToolGroupButton directly contains this tool button
        var toolGroupButton = FindToolGroupForButton(toolButton);
        if (toolGroupButton == null)
        {
            return null;
        }

        // Find the ModdableToolGroupButton that wraps this ToolGroupButton
        return customBlockObjectButtons.ToolGroupButtonsById.Values
            .FirstOrDefault(mgb => ReferenceEquals(mgb.ToolGroupButton, toolGroupButton));
    }

    ToolGroupButton? FindToolGroupForButton(IToolbarButton button)
    {
        // Find which ToolGroupButton contains this button
        foreach (var groupButton in toolButtonService._toolGroupButtons)
        {
            foreach (var toolButton in groupButton.ToolButtons)
            {
                if (ReferenceEquals(toolButton, button))
                {
                    return groupButton;
                }
            }
        }

        return null;
    }

    void RegisterAction(string id, Action onDown)
    {
        var ev = keyBindingEventService.Get(id);
        ev.OnDown += onDown;
    }

}
