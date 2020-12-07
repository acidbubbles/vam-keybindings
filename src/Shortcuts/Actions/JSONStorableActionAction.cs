public class JSONStorableActionAction : IAction
{
    // TODO: If many mapped, use last selected
    public JSONStorable storable { get; set; }
    public string name => action.name;
    public string label => action.name;
    public JSONStorableAction action;

    public void Invoke()
    {
        action.actionCallback.Invoke();
    }
}
