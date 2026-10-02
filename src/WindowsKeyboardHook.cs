using System.ComponentModel;

namespace HolzShots.Input.Keyboard;

public sealed class WindowsKeyboardHook : KeyboardHook
{
    private readonly HotkeyWindowHost _window;
    private readonly ISynchronizeInvoke _synchronizer;

    /// <summary>Creates a hook whose native window lives on the thread of <paramref name="synchronizer"/>.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="synchronizer"/> is <see langword="null"/>.</exception>
    /// <exception cref="Win32Exception">The native window could not be created.</exception>
    /// <exception cref="InvalidOperationException">The synchronizer cannot marshal the call, e.g. a <c>Control</c> without a window handle.</exception>
    public WindowsKeyboardHook(ISynchronizeInvoke synchronizer)
    {
        ArgumentNullException.ThrowIfNull(synchronizer);

        _synchronizer = synchronizer;
        // RegisterHotKey fails with ERROR_WINDOW_OF_OTHER_THREAD for windows created by another thread,
        // and WM_HOTKEY is dispatched by the message loop of the window's thread. So the window must live on the synchronizer's thread.
        _window = InvokeWrapper(() => new HotkeyWindowHost());
        _window.KeyPressed += KeyPressed; // register the event of the inner native window.
    }

    /// <summary>Registers a hotkey in the system.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="hotkey"/> is <see langword="null"/>.</exception>
    /// <exception cref="HotkeyRegistrationException">
    /// The hotkey is already registered (inner exception: <see cref="InvalidOperationException"/>),
    /// or the system refused the registration (inner exception: <see cref="Win32Exception"/>). The hook is left unchanged.
    /// </exception>
    /// <exception cref="InvalidOperationException">The synchronizer cannot marshal the call, e.g. a <c>Control</c> without a window handle.</exception>
    public override void RegisterHotkey(Hotkey hotkey) => InvokeWrapper(() => RegisterHotkeyInternal(hotkey));

    private void RegisterHotkeyInternal(Hotkey hotkey)
    {
        ArgumentNullException.ThrowIfNull(hotkey);

        var id = hotkey.GetHashCode();
        if (!RegisteredKeys.TryAdd(id, hotkey))
            throw new HotkeyRegistrationException(hotkey, new InvalidOperationException("Hotkey already registered."));

        try
        {
            _window.RegisterHotkey(hotkey.Modifiers, hotkey.Key, id);
        }
        catch (Win32Exception ex)
        {
            RegisteredKeys.Remove(id); // Rollback the add
            throw new HotkeyRegistrationException(hotkey, ex);
        }
    }

    /// <summary>Unregisters a hotkey in the system.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="hotkey"/> is <see langword="null"/>.</exception>
    /// <exception cref="HotkeyRegistrationException">
    /// The hotkey is not registered (inner exception: <see cref="InvalidOperationException"/>),
    /// or the system failed to unregister it (inner exception: <see cref="Win32Exception"/>). The hotkey stays registered and keeps its subscribers.
    /// </exception>
    /// <exception cref="InvalidOperationException">The synchronizer cannot marshal the call, e.g. a <c>Control</c> without a window handle.</exception>
    public override void UnregisterHotkey(Hotkey hotkey) => InvokeWrapper(() => UnregisterHotkeyInternal(hotkey));

    private void UnregisterHotkeyInternal(Hotkey hotkey)
    {
        ArgumentNullException.ThrowIfNull(hotkey);

        var id = hotkey.GetHashCode();
        if (!RegisteredKeys.Remove(id))
            throw new HotkeyRegistrationException(hotkey, new InvalidOperationException("Hotkey not registered."));

        try
        {
            _window.UnregisterHotkey(id);
            hotkey.RemoveAllEventHandlers();
        }
        catch (Win32Exception ex)
        {
            RegisteredKeys.Add(id, hotkey); // Rollback the remove
            throw new HotkeyRegistrationException(hotkey, ex);
        }
    }

    /// <summary>Unregisters all hotkeys. Every hotkey is attempted; failures are collected and thrown afterwards.</summary>
    /// <exception cref="HotkeyRegistrationException">
    /// At least one hotkey could not be unregistered. The inner exception is an <see cref="AggregateException"/> with the individual
    /// <see cref="HotkeyRegistrationException"/>s; hotkeys that failed stay registered.
    /// </exception>
    /// <exception cref="InvalidOperationException">The synchronizer cannot marshal the call, e.g. a <c>Control</c> without a window handle.</exception>
    public override void UnregisterAllHotkeys() => InvokeWrapper(() =>
    {
        List<HotkeyRegistrationException>? failures = null;

        var toUnregister = new List<Hotkey>(RegisteredKeys.Values);
        foreach (var hotkey in toUnregister)
        {
            try
            {
                UnregisterHotkeyInternal(hotkey);
            }
            catch (HotkeyRegistrationException ex)
            {
                (failures ??= []).Add(ex);
            }
        }

        if (failures is not null)
            throw new HotkeyRegistrationException($"Failed to unregister {failures.Count} hotkey(s).", new AggregateException(failures));
    });

    /// <summary>Runs <paramref name="action"/> synchronously on the synchronizer's thread, so exceptions reach the caller.</summary>
    private void InvokeWrapper(Action action)
    {
        if (!_synchronizer.InvokeRequired)
        {
            action();
            return;
        }
        _synchronizer.Invoke(action, null);
    }

    private T InvokeWrapper<T>(Func<T> func) => _synchronizer.InvokeRequired
        ? (T)_synchronizer.Invoke(func, null)!
        : func();

    /// <remarks>Failing to unregister hotkeys does not throw (see <see cref="KeyboardHook.Dispose()"/>).</remarks>
    /// <exception cref="InvalidOperationException">The synchronizer cannot marshal the call, e.g. a <c>Control</c> without a window handle.</exception>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); // unregisters all hotkeys
        if (!disposing)
            return;

        InvokeWrapper(() =>
        {
            _window.KeyPressed -= KeyPressed;
            _window.Dispose(); // the native window must be destroyed on the thread that owns it.
        });
    }
}
