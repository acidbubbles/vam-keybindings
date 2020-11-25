public class BindableActions : MVRScript
{
    public override void Init()
    {
        CreateTextField(new JSONStorableString("Description", "This plugin is used for bindings. It offers additional shortcuts not otherwise available using Virt-A-Mate triggers."));

        RegisterString(new JSONStorableString("Log Message", null, SuperController.LogMessage)
        {
            isStorable = false,
            isRestorable = false
        });

        RegisterAction(new JSONStorableAction("Clear Message Log", SuperController.singleton.ClearMessages));

        RegisterString(new JSONStorableString("Log Error", null, SuperController.LogError)
        {
            isStorable = false,
            isRestorable = false
        });

        RegisterAction(new JSONStorableAction("Clear Error Log", SuperController.singleton.ClearErrors));
    }
}
