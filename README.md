# Virt-A-Mate Keybindings

> Under development

- Create custom keybindings for Virt-A-Mate
- Invoke custom triggers in your scenes
- Fuzzy find commands using autocomplete
- Use vim-like key sequences or vscode-like shortcuts
- Access additional built-in features like selection history

## How to use Keybindings

`Keybindings.cslist` is the main plugin, and should be installed as a session plugin. This is what controls the keyboard shortcuts and provides the "shared commands", e.g. adding atoms and opening panels.

1. Add `Keybindings.cslist` in your Session Plugins. You can automatically load it by going to Session Plugin Presets and selecting "Change User Defaults".
2. Open the `Keybindings` custom UI. By default, nothing will be bound. You can import some, or create your own. You should at least bind the `FindCommand` command. For example, let's bind it to `F12`. Note that the box will be red for one second. You can type multiple keys if you want to use a multi-key binding, and you can use `Ctrl`, `Shift` and `Alt` if you want.
3. Now press `F12` (or the shortcut you have chosen). On the bottom left you should see a text field. Try typing `add`, and you'll see a command appear. Continue typing to filter out commands, `Tab` to cycle through the commands, or `Shift+Tab` to cycle back. Press `Enter` to execute the command, or `Esc` to cancel. This uses fuzzy searching, so you can type "oppclo" to find "Open_PersonAtom_ClothingTab" quickly.
4. You can quickly open the keybindings at any time by finding the command `KeybindingsSettings`.

## How to use custom triggers

`CustomCommands.cslist` can be used to create commands in your scene that you can bind. For example, you could bind "F5" to "Play_Animation", and if your scene contains an atom with this command, it will be executed. If more than one atom has this command, the current or the last atom containing this command will be executed.

1. Add `CustomCommands.cslist` to an atom in your scene.
2. Open the `CustomCommands` custom UI. The list of commands will be empty.
3. Press `Add a custom command`. This will create an unnamed trigger. You _must_ name your commands for them to be invokable by Keybindings.
4. Press `Edit` on the command you have just created. It will open the Virt-A-Mate Trigger interface.
5. Enter a name. This is the name that will be used to invoke your command. Let's call it `Log_HelloWorld`
6. Choose a trigger. By default, the current atom will be automatically selected. You can also look at the `ParameterizedTriggers` storable receiver for additional functionality such as logging messages, quick-selecting this atom and reloading plugins.
7. You can use the Find Commands feature (`F12` if you followed the previous tutorial) to invoke your new command. Type `hello` and you should see your command. You can also bind it now in the `Keybindings` custom UI.

## Gotchas

This is still being developed! Here are some gotchas:

- A lot of keybindings are controlled by Virt-A-Mate. The next version will allow Keybindings to take control over those shortcuts too, thanks to [Meshed being very responsive](https://hub.virtamate.com/threads/1-20-1-6-ability-to-disable-or-override-built-in-shortcuts-quick-win.3841/#post-9675)!
- If you bound a shortcut to a scene command, when that command is not available the binding will be "hidden" and cannot be edited.
- Only actions can be invoked. Eventually floats and toggles could be implemented.
- There is no search (yet) in the keybindings screen, so there might be some scrolling involved.
- Not all useful commands have been implemented, let me know if some of your favorites might be missing! Naming is also subject to change.

## License

[MIT](LICENSE.md)
