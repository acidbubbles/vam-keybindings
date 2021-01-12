public class JSONStorableBoolCommandInvoker : CommandInvokerBase, IActionCommandInvoker, ICommandReleaser
{
    private readonly JSONStorableBool _action;

    public JSONStorableBoolCommandInvoker(JSONStorable storable, string ns, string localName, JSONStorableBool action)
        : base(storable, ns, localName)
    {
        _action = action;
    }

    public ICommandReleaser Invoke()
    {
        _action.val = true;
        return this;
    }

    public void Release()
    {
        _action.val = false;
    }
}
