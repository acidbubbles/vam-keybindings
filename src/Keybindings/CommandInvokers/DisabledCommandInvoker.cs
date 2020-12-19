public class DisabledCommandInvoker : ICommandInvoker
{
    public JSONStorable storable => null;
    public string commandName { get; }
    public string ns { get; }
    public string localName { get; }

    public DisabledCommandInvoker(string commandName)
    {
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

    public void Invoke()
    {
    }
}
