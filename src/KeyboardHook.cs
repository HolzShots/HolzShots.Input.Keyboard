using System.Diagnostics;

namespace HolzShots.Input.Keyboard;

public abstract class KeyboardHook : IDisposable
{
    protected Dictionary<int, Hotkey> RegisteredKeys { get; } = [];

    /// <summary>Registers a hotkey in the system.</summary>
    public abstract void RegisterHotkey(Hotkey hotkey);

    /// <summary>Unregisters a hotkey in the system.</summary>
    /// <remarks>All <see cref="Hotkey.KeyPressed"/> subscribers of the hotkey are removed, so the same instance can be registered again with a clean slate.</remarks>
    public abstract void UnregisterHotkey(Hotkey hotkey);

    /// <summary>Unregisters all hotkeys registered with this hook. See <see cref="UnregisterHotkey"/> for the effect on their subscribers.</summary>
    public abstract void UnregisterAllHotkeys();

    protected void KeyPressed(object? sender, KeyPressedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (RegisteredKeys.TryGetValue(args.Id, out var hk))
            hk.InvokePressed(this);
    }

    #region IDisposable Members

    private bool _isDisposed;
    public void Dispose()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <remarks>Only called once. Derived classes must not touch other managed objects when <paramref name="disposing"/> is false.</remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        try
        {
            UnregisterAllHotkeys();
        }
        catch (HotkeyRegistrationException ex)
        {
            // Best effort: Dispose must not throw.
            Trace.WriteLine($"Failed to unregister hotkeys during dispose: {ex}");
        }
    }

    #endregion
}
