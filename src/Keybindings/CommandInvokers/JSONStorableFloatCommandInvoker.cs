public class JSONStorableFloatCommandInvoker : CommandInvokerBase, IAnalogCommandInvoker
{
    public string buttonLabel => null;

    private readonly JSONStorableFloat _storableFloat;

    public JSONStorableFloatCommandInvoker(JSONStorable storable, string ns, string localName, JSONStorableFloat storableFloat)
        : base(storable, ns, localName)
    {
        _storableFloat = storableFloat;
    }

    public void UpdateValue(float value)
    {
        _storableFloat.val = value;
    }
}
