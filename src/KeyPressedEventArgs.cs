namespace HolzShots.Input.Keyboard;

/// <summary>Event Args for the event that is fired after the hotkey has been pressed.</summary>
public class KeyPressedEventArgs(int id, ModifierKeys modifier, Keys key) : EventArgs
{
    /// <summary>The id the hotkey was registered with.</summary>
    public int Id { get; } = id;
    public ModifierKeys Modifier { get; } = modifier;
    public Keys Key { get; } = key;
}
