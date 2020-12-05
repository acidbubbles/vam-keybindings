using SimpleJSON;

public interface IBindingsManager
{
}

public class BindingsManager : IBindingsManager
{
    public BindingTreeNode root { get; } = new BindingTreeNode();

    public JSONClass GetJSON()
    {
        return new JSONClass();
    }

    public void RestoreFromJSON(JSONClass actionsJSON)
    {
    }
}
