using System;

public class ActionCommandInvoker : CommandInvokerBase, ICommandInvoker
{
    private readonly Action _fn;

    public ActionCommandInvoker(JSONStorable storable, string ns, string localName, Action fn)
        : base(storable, ns, localName)
    {
        _fn = fn;
    }

    public void Invoke()
    {
        _fn();
    }
}
