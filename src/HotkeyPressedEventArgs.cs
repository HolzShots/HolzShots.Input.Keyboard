namespace HolzShots.Input.Keyboard;

/// <summary>Event Args for the event that is fired after the hotkey has been pressed.</summary>
public class HotkeyPressedEventArgs(KeyboardHook hook, Hotkey hotkey) : EventArgs
{
    public Hotkey Hotkey { get; } = hotkey;
    public KeyboardHook Hook { get; } = hook;
}
