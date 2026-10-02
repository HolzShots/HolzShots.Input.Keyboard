using System.ComponentModel;

namespace HolzShots.Input.Keyboard;

public static class KeyboardHookSelector
{
    public static KeyboardHook CreateHookForCurrentPlatform(ISynchronizeInvoke invoke) =>
        OperatingSystem.IsWindows()
            ? new WindowsKeyboardHook(invoke)
            : throw new PlatformNotSupportedException($"Unhandled platform: {Environment.OSVersion.Platform}");
}
