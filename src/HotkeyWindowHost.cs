using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace HolzShots.Input.Keyboard;

/// <summary>A message-only window that receives WM_HOTKEY.</summary>
/// <remarks>Must be created, used and disposed on the same thread, and that thread must run a message loop.</remarks>
internal sealed unsafe class HotkeyWindowHost : IDisposable
{
    private const uint WM_HOTKEY = 0x0312;
    private const nint HWND_MESSAGE = -3;

    private static readonly Lazy<ushort> WindowClass = new(RegisterWindowClass);

    /// <summary>Maps the window handles to their hosts, so the static window procedure can dispatch to the instance.</summary>
    private static readonly ConcurrentDictionary<nint, HotkeyWindowHost> Hosts = new();

    private nint _handle;

    public HotkeyWindowHost()
    {
        _handle = NativeMethods.CreateWindowExW(0, WindowClass.Value, 0, 0, 0, 0, 0, 0, HWND_MESSAGE, 0, NativeMethods.GetModuleHandleW(null), 0);
        if (_handle == 0)
            throw new Win32Exception();

        Hosts[_handle] = this;
    }

    private static ushort RegisterWindowClass()
    {
        // The guid avoids a collision with a class registered by another copy of this assembly in the same process.
        fixed (char* className = $"HolzShots.Input.Keyboard.HotkeyWindowHost.{Guid.NewGuid():N}")
        {
            var wc = new NativeMethods.WNDCLASSEXW
            {
                cbSize = (uint)sizeof(NativeMethods.WNDCLASSEXW),
                lpfnWndProc = &WndProc,
                hInstance = NativeMethods.GetModuleHandleW(null),
                lpszClassName = className,
            };

            var atom = NativeMethods.RegisterClassExW(&wc);
            if (atom == 0)
                throw new Win32Exception();
            return atom;
        }
    }

    [UnmanagedCallersOnly]
    private static nint WndProc(nint hWnd, uint msg, nint wParam, nint lParam)
    {
        if (msg == WM_HOTKEY && Hosts.TryGetValue(hWnd, out var host))
        {
            host.OnHotkey(wParam, lParam);
            return 0;
        }
        return NativeMethods.DefWindowProcW(hWnd, msg, wParam, lParam);
    }

    private void OnHotkey(nint wParam, nint lParam)
    {
        // wParam is the id the hotkey was registered with, lParam carries the key and the modifiers (without MOD_NOREPEAT).
        var id = (int)wParam;
        var key = (Keys)(((int)lParam >> 16) & 0xFFFF);
        var modifier = (ModifierKeys)((int)lParam & 0xFFFF);

        try
        {
            // invoke the event to notify the parent.
            KeyPressed?.Invoke(this, new KeyPressedEventArgs(id, modifier, key));
        }
        catch (Exception ex)
        {
            // An exception unwinding into the native message loop terminates the process. Rethrow it outside of the window procedure instead,
            // so it surfaces the same way as any other exception on this thread (e.g. Application.ThreadException in WinForms).
            var edi = ExceptionDispatchInfo.Capture(ex);
            (SynchronizationContext.Current ?? new SynchronizationContext()).Post(static state => ((ExceptionDispatchInfo)state!).Throw(), edi);
        }
    }

    public void RegisterHotkey(ModifierKeys modifiers, Keys key, int id)
    {
        Trace.WriteLine($"Registering hotkey: {id} {modifiers} {key}");
        if (!NativeMethods.RegisterHotKey(_handle, id, modifiers | ModifierKeys.NoRepeat, (uint)key))
            throw new Win32Exception();
    }

    public void UnregisterHotkey(int id)
    {
        Trace.WriteLine($"Unregistering hotkey: {id}");
        if (!NativeMethods.UnregisterHotKey(_handle, id))
            throw new Win32Exception();
    }

    public event EventHandler<KeyPressedEventArgs>? KeyPressed;

    public void Dispose()
    {
        if (_handle == 0)
            return;

        Hosts.TryRemove(_handle, out _);
        NativeMethods.DestroyWindow(_handle);
        _handle = 0;
    }
}
