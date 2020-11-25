public class BindableActions : MVRScript
{
    public override void Init()
    {
        CreateTextField(new JSONStorableString("Description", "This plugin is used for bindings. It offers additional shortcuts not otherwise available using Virt-A-Mate triggers."));

        RegisterString(new JSONStorableString("Print String", null, SuperController.LogMessage)
        {
            isStorable = false,
            isRestorable = false
        });

        RegisterString(new JSONStorableString("Print Error String", null, SuperController.LogError)
        {
            isStorable = false,
            isRestorable = false
        });
    }
}
