public class JSONStorableActionCommandInvoker : CommandInvokerBase, ICommandInvoker
{
    private readonly JSONStorableAction _action;

    public JSONStorableActionCommandInvoker(JSONStorable storable, string ns, string localName, JSONStorableAction action)
        : base(storable, ns, localName)
    {
        _action = action;
    }

    public void Invoke()
    {
        _action.actionCallback.Invoke();
    }
}
