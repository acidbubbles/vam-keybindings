public class DisabledCommandInvoker : ICommandInvoker
{
    public JSONStorable storable => null;
    public string prettyString { get; }
    public string commandName { get; }
    public string ns { get; }
    public string localName { get; }
    public int slot { get; set; }

    public DisabledCommandInvoker(string commandName, string prettyString, int slot)
    {
        this.commandName = commandName;
        this.prettyString = prettyString;
        this.slot = slot;
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
