using MVR.FileManagementSecure;

// ReSharper disable once InconsistentNaming
public class KeybindingsExtensions_AddPlugin : MVRScript
{
    public override void Init()
    {
        base.Init();

        RegisterUrl(new JSONStorableUrl("Plugin", null, "cs|cslist|dll", "Custom/Scripts")
        {
            beginBrowseWithObjectCallback = jsu =>
            {
                jsu.shortCuts = FileManagerSecure.GetShortCutsForDirectory("Custom/Scripts", true, true, true, true);
            },
            setCallbackFunction = val =>
            {
                // Potential values:
                // - Custom/Scripts/Dev/vam-timeline/VamTimeline.AtomAnimation.cslist
                // - AcidBubbles.Cornwall.2:/Custom/Scripts/AcidBubbles/Cornwall/Cornwall.cs
                SuperController.LogMessage(val);
            },
            fileBrowseButton = CreateButton("Select").button
        });
    }
}
