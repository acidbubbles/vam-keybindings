public abstract class CommandInvokerBase
{
    public JSONStorable storable { get; }
    public string commandName { get; }
    public string ns { get; }
    public string localName { get; }

    protected CommandInvokerBase(JSONStorable storable, string ns, string localName)
    {
        this.storable = storable;
        this.ns = ns;
        this.localName = localName;
        this.commandName = $"{ns}.{localName}";
    }
}
