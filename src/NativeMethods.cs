using System.Runtime.InteropServices;

namespace HolzShots.Input.Keyboard;

internal static partial class NativeMethods
{
    private const string User32 = "user32.dll";

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(nint hWnd, int id, ModifierKeys fsModifiers, Keys vk);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(nint hWnd, int id);
}
