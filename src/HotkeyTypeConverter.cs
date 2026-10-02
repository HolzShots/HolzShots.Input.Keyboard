using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace HolzShots.Input.Keyboard;

public class HotkeyTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(int) || sourceType == typeof(string);
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is int i)
            return Hotkey.FromHashCode(i);
        if (value is string s)
            return Hotkey.TryParse(s, out var hotkey) ? hotkey : null; // An empty or invalid setting means "no hotkey".

        Debug.Fail("Could not convert hotkeys properly. You should debug this."); // Something is wrong here.
        return base.ConvertFrom(context, culture, value);
    }
    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(int) || destinationType == typeof(string);
    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        Debug.Assert(value is Hotkey);
        if (value is Hotkey h)
        {
            if (destinationType == typeof(int))
                return h.GetHashCode();
            if (destinationType == typeof(string))
                return h.ToString();
        }

        Debug.Fail("Could not convert hotkeys properly. You should debug this."); // Something is wrong here.
        return base.ConvertTo(context, culture, value, destinationType);
    }
}
