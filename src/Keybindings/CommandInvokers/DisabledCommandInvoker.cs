public class DisabledCommandInvoker : ICommandInvoker
{
    public string buttonLabel => null;
    public JSONStorable storable => null;
    public string prettyString { get; }
    public string commandName { get; }
    public string ns { get; }
    public string localName { get; }

    public DisabledCommandInvoker(string commandName, string prettyString)
    {
        this.prettyString = prettyString;
        this.commandName = commandName;
        var i = commandName.IndexOf('.');
        if (i == -1 || i == commandName.Length - 1)
        {
            ns = "INVALID";
            localName = commandName;
        }
        else
        {
            ns = commandName.Substring(0, i);
            localName = commandName.Substring(i + 1);
        }
    }
}
