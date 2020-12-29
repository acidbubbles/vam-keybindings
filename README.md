# Virt-A-Mate Keybindings

> Under development

- Navigate Virt-A-Mate's UI easily
- Create custom keybindings for Virt-A-Mate
- Invoke custom triggers in your scenes
- Fuzzy find commands using autocomplete
- Use vim-like key sequences or vscode-like shortcuts
- Access additional built-in features like selection history
- Use your keyboard or gamepad
- Integrates with Timeline

## How to use Keybindings

`Keybindings.cslist` is the main plugin, and should be installed as a session plugin. This is what controls the keyboard shortcuts and provides the "shared commands", e.g. adding atoms and opening panels.

1. Add `Keybindings.cslist` in your Session Plugins. You can automatically load it by going to Session Plugin Presets and selecting "Change User Defaults".
2. Open the `Keybindings` custom UI. By default, nothing will be bound. You can import some, or create your own. You should at least bind the `FindCommand` command. For example, let's bind it to <kbd>F12</kbd>. Note that the box will be red for one second. You can type multiple keys if you want to use a multi-key binding, and you can use <kbd>Ctrl</kbd>, <kbd>Shift</kbd> and <kbd>Alt</kbd> if you want.
3. Now press <kbd>F12</kbd> (or the shortcut you have chosen). On the bottom left you should see a text field. Try typing `add`, and you'll see a command appear. Continue typing to filter out commands, <kbd>Tab</kbd> to cycle through the commands, or <kbd>Shift</kbd> + <kbd>Tab</kbd> to cycle back. Press <kbd>Enter</kbd> to execute the command, or <kbd>Esc</kbd> to cancel. This uses fuzzy searching, so you can type "oppclo" to find "Open_PersonAtom_ClothingTab" quickly.
4. You can quickly open the keybindings at any time by finding the command `KeybindingsSettings`.

Bindings with an asterisk (`*`) can be bound to joysticks and mouse. You can use modifier keys too, for example you can use <kbd>Alt</kbd> + Mouse X by holding the <kbd>Alt</kbd> key and moving the mouse to the left or to the right.

## How to use custom triggers

`CustomCommands.cslist` can be used to create commands in your scene that you can bind. For example, you could bind <kbd>F5</kbd> to "Play_Animation", and if your scene contains an atom with this command, it will be executed. If more than one atom has this command, the current or the last atom containing this command will be executed.

1. Add `CustomCommands.cslist` to an atom in your scene.
2. Open the `CustomCommands` custom UI. The list of commands will be empty.
3. Press `Add a custom command`. This will create an unnamed trigger. You _must_ name your commands for them to be invokable by Keybindings.
4. Press `Edit` on the command you have just created. It will open the Virt-A-Mate Trigger interface.
5. Enter a name. This is the name that will be used to invoke your command. Let's call it `Log_HelloWorld`
6. Choose a trigger. By default, the current atom will be automatically selected. You can also look at the `plugin#0_CustomCommands` storable receiver for additional functionality such as logging messages, quick-selecting this atom and reloading plugins.
7. You can use the Find Commands feature (<kbd>F12</kbd> if you followed the previous tutorial) to invoke your new command. Type `hello` and you should see your command. You can also bind it now in the `Keybindings` custom UI.

## Gotchas

This plugin is still being actively developed! Here are some gotchas:

- A lot of keybindings are controlled by Virt-A-Mate. The next version will allow Keybindings to take control over those shortcuts too, thanks to [Meshed being very responsive](https://hub.virtamate.com/threads/1-20-1-6-ability-to-disable-or-override-built-in-shortcuts-quick-win.3841/#post-9675)!
- Not all useful commands have been implemented, let me know if some of your favorites might be missing! Naming is also subject to change.
- Opened plugin UIs will not be closed, and will therefore stack.


## Integrating your plugins

You can publish your own commands to be used by Keybindings. It will use the [vam-plugins-interop-specs](https://github.com/vam-community/vam-plugins-interop-specs) specifications.

In short, you simply need to call `OnActionsProviderAvailable` when your plugin is ready to receive shortcuts, and implement `OnBindingsListRequested` to add the `JSONStorableAction` you want to make available.

```c#
public class YourPlugin : MVRScript
{
    public void Init()
    {
        // Call this when ready to receive shortcuts or when shortcuts have changed
        SuperController.singleton.BroadcastMessage("OnActionsProviderAvailable", this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnDestroy()
    {
        // Call this when this plugin should not receive shortcuts anymore
        SuperController.singleton.BroadcastMessage("OnActionsProviderDestroyed", this, SendMessageOptions.DontRequireReceiver);
    }

    // This method will be called by Keybindings when it is ready.
    public void OnBindingsListRequested(List<object> bindings)
    {
        bindings.Add(new JSONStorableAction("SayHi", () => SuperController.LogMessage("Hi!")));
    }
}
```

You can also optionally provide additional settings to Keybindings by returning an `IEnumerable<string, string>` in the `OnBindingListRequested` method:

```c#
    public void OnBindingsListRequested(List<object> bindings)
    {
        bindings.Add(new Dictionary<string, string>
        {
            // This will determine the "prefix" for your commands, e.g. MySuperPlugin.SayHi
            { "Namespace", "MySuperPlugin" }
        });
        bindings.Add(new JSONStorableAction("SayHi", () => SuperController.LogMessage("Hi!")));
    }
```

Supported storables:

- `JSONStorableAction` will be invoked on key down
- `JSONStorableFloat` will receive values between -1 and 1 for joysticks, or the mouse position delta. You should check for their current value in `Update()`.

## Credits

- [LFE](https://github.com/lfe999) who made the original Keyboard Shortcuts plugin and provided his insight
- [Hazmox](https://hub.virtamate.com/members/hazmhox.351/) who made overlays work in his VAMOverlays plugin
- [MacGruber](https://www.patreon.com/MacGruber_Laboratory/) who's always a great source of inspiration and a pioneer
- All of my [patrons](https://www.patreon.com/acidbubbles) and vammers who give meaning to what I do!",

## License

[MIT](LICENSE.md)
