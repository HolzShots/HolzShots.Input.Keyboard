using System.ComponentModel;
using System.Diagnostics;

namespace HolzShots.Input.Keyboard;

internal class HotkeyWindowHost : NativeWindow, IDisposable
{
    public HotkeyWindowHost() => CreateHandle(new CreateParams()); // create the handle for the window.

    public sealed override void CreateHandle(CreateParams cp) => base.CreateHandle(cp);

    /// <summary>Overridden to get the notifications.</summary>
    protected override void WndProc(ref Message m)
    {
        const int WM_HOTKEY = 0x0312;

        // check if we got a hotkey pressed.
        if (m.Msg == WM_HOTKEY)
        {
            // wParam is the id the hotkey was registered with, lParam carries the key and the modifiers (without MOD_NOREPEAT).
            var id = (int)m.WParam;
            var key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
            var modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

            // invoke the event to notify the parent.
            KeyPressed?.Invoke(this, new KeyPressedEventArgs(id, modifier, key));
            return;
        }
        base.WndProc(ref m);
    }

    public void RegisterHotkey(ModifierKeys modifiers, Keys key, int id)
    {
        Trace.WriteLine($"Registering hotkey: {id} {modifiers} {key}");
        if (!NativeMethods.RegisterHotKey(Handle, id, modifiers | ModifierKeys.NoRepeat, key))
            throw new Win32Exception();
    }

    public void UnregisterHotkey(int id)
    {
        Trace.WriteLine($"Unregistering hotkey: {id}");
        if (!NativeMethods.UnregisterHotKey(Handle, id))
            throw new Win32Exception();
    }

    public event EventHandler<KeyPressedEventArgs>? KeyPressed;

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        DestroyHandle();
        GC.SuppressFinalize(this); // NativeWindow has a finalizer; nothing is left for it to do.
    }
}
