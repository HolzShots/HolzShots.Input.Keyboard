using System.Runtime.InteropServices;

namespace HolzShots.Input.Keyboard;

internal static unsafe partial class NativeMethods
{
    private const string User32 = "user32.dll";
    private const string Kernel32 = "kernel32.dll";

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool RegisterHotKey(nint hWnd, int id, ModifierKeys fsModifiers, uint vk);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool UnregisterHotKey(nint hWnd, int id);

    [LibraryImport(User32, SetLastError = true)]
    internal static partial ushort RegisterClassExW(WNDCLASSEXW* lpwcx);

    // lpClassName is a class atom (MAKEINTATOM).
    [LibraryImport(User32, SetLastError = true)]
    internal static partial nint CreateWindowExW(uint dwExStyle, nint lpClassName, nint lpWindowName, uint dwStyle,
        int x, int y, int nWidth, int nHeight, nint hWndParent, nint hMenu, nint hInstance, nint lpParam);

    [LibraryImport(User32, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DestroyWindow(nint hWnd);

    [LibraryImport(User32)]
    internal static partial nint DefWindowProcW(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport(Kernel32, SetLastError = true)]
    internal static partial nint GetModuleHandleW(char* lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    internal struct WNDCLASSEXW
    {
        public uint cbSize;
        public uint style;
        public delegate* unmanaged<nint, uint, nint, nint, nint> lpfnWndProc;
        public int cbClsExtra;
        public int cbWndExtra;
        public nint hInstance;
        public nint hIcon;
        public nint hCursor;
        public nint hbrBackground;
        public char* lpszMenuName;
        public char* lpszClassName;
        public nint hIconSm;
    }
}
