using System.ComponentModel;

namespace HolzShots.Input.Keyboard;

public static class KeyboardHookSelector
{
    /// <summary>Creates the <see cref="KeyboardHook"/> implementation for the operating system the process runs on.</summary>
    /// <exception cref="PlatformNotSupportedException">The current platform has no keyboard hook implementation (only Windows is supported).</exception>
    /// <exception cref="ArgumentNullException"><paramref name="invoke"/> is <see langword="null"/> (thrown on Windows).</exception>
    /// <exception cref="System.ComponentModel.Win32Exception">The native window of the hook could not be created (thrown on Windows).</exception>
    public static KeyboardHook CreateHookForCurrentPlatform(ISynchronizeInvoke invoke) =>
        OperatingSystem.IsWindows()
            ? new WindowsKeyboardHook(invoke)
            : throw new PlatformNotSupportedException($"Unhandled platform: {Environment.OSVersion.Platform}");
}
