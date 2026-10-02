# `HolzShots.Input.Keyboard`
> Global Hotkey library used in [HolzShots](https://github.com/nikeee/HolzShots).


Usage of `Hotkey` primitives:
```csharp
var hook = KeyboardHookSelector.CreateHookForCurrentPlatform(someForm);
// someForm can be anything that implements ISynchronizeInvoke.
// The hook's native window lives on that object's thread, and all registrations are marshalled to it.

var hk = new Hotkey(ModifierKeys.Shift, Keys.F8);
hk.KeyPressed += (sender, e) => Console.WriteLine($"Hotkey pressed: {e.Hotkey}");

hook.RegisterHotkey(hk);

var hk2 = Hotkey.Parse("Shift+F9"); // throws FormatException on invalid input, Hotkey.TryParse does not
hk2.KeyPressed += (sender, e) => Console.WriteLine($"Hotkey pressed: {e.Hotkey}");

hook.RegisterHotkey(hk2);

// Clean up:
hook.UnregisterHotkey(hk2); // also removes all KeyPressed subscribers of hk2

hook.UnregisterAllHotkeys();
```

The `Hotkey` class can be serialized to Windows Forms Settings as well as re-instantiated using `Hotkey.Parse(hk.ToString())`.
You may also implement a JSON converter to parse hotkey strings from JSON config files.
