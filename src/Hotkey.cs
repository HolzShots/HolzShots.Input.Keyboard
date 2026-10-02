using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HolzShots.Input.Keyboard;

[SettingsSerializeAs(SettingsSerializeAs.String)]
[TypeConverter(typeof(HotkeyTypeConverter))]
public class Hotkey : IEquatable<Hotkey>
{
    private const char KeySeparator = '+';

    public ModifierKeys Modifiers { get; }
    public Keys Key { get; }

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

    /// <summary>Parses a hotkey string such as <c>Ctrl+Shift+F8</c>. See <see cref="TryParse"/> for the format.</summary>
    /// <exception cref="FormatException">The string is not a valid hotkey.</exception>
    public static Hotkey Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return TryParse(value, out var hotkey)
            ? hotkey
            : throw new FormatException($"'{value}' is not a valid hotkey.");
    }

    /// <summary>
    /// Parses a hotkey string such as <c>Ctrl+Shift+F8</c>.
    /// Modifiers are <c>Ctrl</c>/<c>Control</c>, <c>Alt</c>, <c>Shift</c> and <c>Win</c>/<c>Super</c>; the key is any member of <see cref="Keys"/>.
    /// Exactly one key is required, tokens are separated by <c>+</c>, matching is case-insensitive.
    /// </summary>
    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out Hotkey? hotkey)
    {
        hotkey = null;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var modifiers = ModifierKeys.None;
        Keys? key = null;

        foreach (var token in value.Split(KeySeparator))
        {
            switch (token.Trim().ToLowerInvariant())
            {
                case "ctrl":
                case "control":
                    modifiers |= ModifierKeys.Control;
                    continue;
                case "alt":
                    modifiers |= ModifierKeys.Alt;
                    continue;
                case "win":
                case "super":
                    modifiers |= ModifierKeys.Win;
                    continue;
                case "shift":
                    modifiers |= ModifierKeys.Shift;
                    continue;
                case var keyName:
                    if (key is not null || !TryParseKey(keyName, out var parsed))
                        return false; // more than one key, or not a key at all
                    key = parsed;
                    continue;
            }
        }

        if (key is null)
            return false;

        hotkey = new Hotkey(modifiers, key.Value);
        return true;
    }

    private static bool TryParseKey(string name, out Keys key)
    {
        key = Keys.None;

        // Enum.TryParse also accepts numbers and comma-separated flag lists. Neither is a key name.
        if (name.Length == 0 || char.IsAsciiDigit(name[0]) || name[0] == '-' || name.Contains(','))
            return false;

        return Enum.TryParse(name, ignoreCase: true, out key) && (key & ~Keys.KeyCode) == 0;
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

    /// <summary>The hotkey has been pressed. All subscribers are removed when the hotkey is unregistered from a <see cref="KeyboardHook"/>.</summary>
    public event EventHandler<HotkeyPressedEventArgs>? KeyPressed;

    internal void RemoveAllEventHandlers() => KeyPressed = null;

    internal void InvokePressed(KeyboardHook hook) => KeyPressed?.Invoke(this, new HotkeyPressedEventArgs(hook, this));
}
