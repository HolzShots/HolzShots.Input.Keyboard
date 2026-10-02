using System.ComponentModel;
using System.Configuration;
using System.Text;

namespace HolzShots.Input.Keyboard;

[Serializable]
[SettingsSerializeAs(SettingsSerializeAs.String)]
[TypeConverter(typeof(HotkeyTypeConverter))]
public class Hotkey : IEquatable<Hotkey>
{
    private const char KeySeparator = '+';

    public ModifierKeys Modifiers { get; }
    public Keys Key { get; }

    /// <summary> Do not use. Only there for serialization. </summary>
    private Hotkey() { }

    public Hotkey(ModifierKeys modifiers, Keys key)
    {
        if ((key & ~Keys.KeyCode) != 0)
            throw new ArgumentOutOfRangeException(nameof(key), key, "Key must not contain modifier bits. Use the modifiers parameter instead.");

        Modifiers = modifiers;
        Key = key;
    }

    public bool IsNone() => Modifiers == ModifierKeys.None && Key == Keys.None;

    public override bool Equals(object? obj) => Equals(obj as Hotkey);
    public bool Equals(Hotkey? other) => other is not null && Modifiers == other.Modifiers && Key == other.Key;

    public override int GetHashCode() => (int)Key << 16 | (int)Modifiers;

    internal static Hotkey FromHashCode(int hashCode)
    {
        var key = hashCode >>> 16;
        var mod = hashCode & 0xFFFF;
        return new Hotkey((ModifierKeys)mod, (Keys)key);
    }

    public static Hotkey? Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var mods = ModifierKeys.None;
        var keys = Keys.None;

        var split = value.Split(KeySeparator);
        foreach (var key in split)
        {
            var keyStr = key.Trim().ToLowerInvariant();
            switch (keyStr)
            {
                case "ctrl":
                case "control":
                    mods |= ModifierKeys.Control;
                    continue;
                case "alt":
                    mods |= ModifierKeys.Alt;
                    continue;
                case "win":
                case "super":
                    mods |= ModifierKeys.Win;
                    continue;
                case "shift":
                    mods |= ModifierKeys.Shift;
                    continue;
            }

            if (Enum.TryParse(keyStr, true, out keys))
                break;
        }

        return new Hotkey(mods, keys);
    }
    public static Hotkey FromKeyboardEvent(KeyEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        var key = e.KeyCode;

        // A modifier key pressed on its own is the hotkey itself, not a modifier of something else.
        // KeyCode is never Keys.None in that case; it is the virtual key of the modifier.
        if (key is Keys.ControlKey or Keys.LControlKey or Keys.RControlKey
                or Keys.ShiftKey or Keys.LShiftKey or Keys.RShiftKey
                or Keys.Menu or Keys.LMenu or Keys.RMenu)
            return new Hotkey(ModifierKeys.None, key);

        var modifiers = ModifierKeys.None;
        if (e.Control)
            modifiers |= ModifierKeys.Control;
        if (e.Shift)
            modifiers |= ModifierKeys.Shift;
        if (e.Alt)
            modifiers |= ModifierKeys.Alt;

        return new Hotkey(modifiers, key);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Modifiers.HasFlag(ModifierKeys.Alt))
            sb.Append("Alt").Append(KeySeparator);
        if (Modifiers.HasFlag(ModifierKeys.Control))
            sb.Append("Ctrl").Append(KeySeparator);
        if (Modifiers.HasFlag(ModifierKeys.Shift))
            sb.Append("Shift").Append(KeySeparator);
        if (Modifiers.HasFlag(ModifierKeys.Win))
            sb.Append("Win").Append(KeySeparator);

        sb.Append(Key.ToString());
        return sb.ToString();
    }

    /// <summary>The hotkey has been pressed.</summary>
    public event EventHandler<HotkeyPressedEventArgs>? KeyPressed;

    internal void RemoveAllEventHandlers() => KeyPressed = null;

    internal void InvokePressed(KeyboardHook hook) => KeyPressed?.Invoke(this, new HotkeyPressedEventArgs(hook, this));
}
