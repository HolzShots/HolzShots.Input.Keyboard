using System.ComponentModel;

namespace HolzShots.Input.Keyboard;

public static class KeyboardHookSelector
{
    public static KeyboardHook CreateHookForCurrentPlatform(ISynchronizeInvoke invoke) =>
        Environment.OSVersion.Platform switch
        {
            PlatformID.Win32S or PlatformID.Win32Windows or PlatformID.Win32NT or PlatformID.WinCE
                => new WindowsKeyboardHook(invoke),
            _ => throw new NotSupportedException($"Unhandled platform: {Environment.OSVersion.Platform}")
        };
}
