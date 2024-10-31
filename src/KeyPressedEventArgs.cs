namespace HolzShots.Input.Keyboard;

/// <summary>Event Args for the event that is fired after the hotkey has been pressed.</summary>
public class KeyPressedEventArgs(ModifierKeys modifier, Keys key) : EventArgs
{
    public ModifierKeys Modifier { get; } = modifier;
    public Keys Key { get; } = key;

    public int GetIdentifier() => (int)Key << 16 | (int)Modifier;
}
