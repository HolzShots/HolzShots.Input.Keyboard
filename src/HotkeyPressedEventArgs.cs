using System.Diagnostics;

namespace HolzShots.Input.Keyboard;

/// <summary>Event Args for the event that is fired after the hotkey has been pressed.</summary>
public class HotkeyPressedEventArgs(KeyboardHook hook, Hotkey hotkey) : EventArgs
{
    public Hotkey Hotkey { get; } = hotkey;
    public KeyboardHook Hook { get; } = hook;

    [Conditional("DEBUG")]
    private static void ValidateArgs(KeyboardHook hook, Hotkey hotkey)
    {
        Debug.Assert(hook is not null);
        Debug.Assert(hotkey is not null);
    }
}
