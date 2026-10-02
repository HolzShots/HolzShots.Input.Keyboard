using System.Diagnostics;

namespace HolzShots.Input.Keyboard;

public abstract class KeyboardHook : IDisposable
{
    protected Dictionary<int, Hotkey> RegisteredKeys { get; } = [];

    /// <summary>Registers a hotkey in the system.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="hotkey"/> is <see langword="null"/>.</exception>
    /// <exception cref="HotkeyRegistrationException">
    /// The hotkey is already registered with this hook (inner exception: <see cref="InvalidOperationException"/>),
    /// or the system refused the registration, e.g. because another application already owns the key combination (inner exception: <see cref="System.ComponentModel.Win32Exception"/>).
    /// The hook is left unchanged in both cases.
    /// </exception>
    public abstract void RegisterHotkey(Hotkey hotkey);

    /// <summary>Unregisters a hotkey in the system.</summary>
    /// <remarks>All <see cref="Hotkey.KeyPressed"/> subscribers of the hotkey are removed, so the same instance can be registered again with a clean slate.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="hotkey"/> is <see langword="null"/>.</exception>
    /// <exception cref="HotkeyRegistrationException">
    /// The hotkey is not registered with this hook (inner exception: <see cref="InvalidOperationException"/>),
    /// or the system failed to unregister it (inner exception: <see cref="System.ComponentModel.Win32Exception"/>).
    /// The hotkey stays registered and keeps its subscribers in both cases.
    /// </exception>
    public abstract void UnregisterHotkey(Hotkey hotkey);

    /// <summary>Unregisters all hotkeys registered with this hook. See <see cref="UnregisterHotkey"/> for the effect on their subscribers.</summary>
    /// <exception cref="HotkeyRegistrationException">
    /// At least one hotkey could not be unregistered. Every hotkey is attempted regardless; the inner exception is an <see cref="AggregateException"/>
    /// with the individual <see cref="HotkeyRegistrationException"/>s. Hotkeys that failed stay registered.
    /// </exception>
    public abstract void UnregisterAllHotkeys();

    /// <exception cref="ArgumentNullException"><paramref name="args"/> is <see langword="null"/>.</exception>
    protected void KeyPressed(object? sender, KeyPressedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (RegisteredKeys.TryGetValue(args.Id, out var hk))
            hk.InvokePressed(this);
    }

    #region IDisposable Members

    private bool _isDisposed;
    /// <summary>Unregisters all hotkeys and releases the resources of the hook.</summary>
    /// <remarks>Best effort: a <see cref="HotkeyRegistrationException"/> while unregistering the hotkeys is traced and not thrown. Calling it more than once has no effect.</remarks>
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
