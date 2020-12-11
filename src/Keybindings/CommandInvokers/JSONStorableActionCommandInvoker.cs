public class JSONStorableActionCommandInvoker : ICommandInvoker
{
    // TODO: If many mapped, use last selected
    public JSONStorable storable { get; set; }
    public string commandName { get; set; }
    public string ns { get; set; }
    public string localName { get; set; }

    public JSONStorableAction action;

    public void Invoke()
    {
        action.actionCallback.Invoke();
    }
}
