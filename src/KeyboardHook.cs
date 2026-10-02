namespace HolzShots.Input.Keyboard;

public abstract class KeyboardHook : IDisposable
{
    protected Dictionary<int, Hotkey> RegisteredKeys { get; } = [];

    /// <summary>Registers a hotkey in the system.</summary>
    public abstract void RegisterHotkey(Hotkey hotkey);

    /// <summary>Unregisters a hotkey in the system.</summary>
    public abstract void UnregisterHotkey(Hotkey hotkey);

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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            UnregisterAllHotkeys();
            _isDisposed = true;
        }
    }

    ~KeyboardHook() => Dispose(false);

    #endregion
}
