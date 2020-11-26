using SimpleJSON;

public interface IBindingsManager
{
}

public class BindingsManager : IBindingsManager
{
    public Binding rootBinding { get; } = new Binding();

    public Binding Add(Binding binding)
    {
        return rootBinding.Add(binding);
    }

    public JSONClass GetJSON()
    {
        return new JSONClass();
    }

    public void RestoreFromJSON(JSONClass actionsJSON)
    {
    }
}
