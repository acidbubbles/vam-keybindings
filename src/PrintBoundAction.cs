using System;

public class PrintBoundAction : IBoundAction
{
    private readonly Func<string> _getMessage;

    public PrintBoundAction(Func<string> getMessage)
    {
        _getMessage = getMessage;
    }

    public void Validate()
    {
    }

    public void SyncAtomNames()
    {
    }

    public void Invoke()
    {
        SuperController.LogMessage(_getMessage());
    }

    public void Edit()
    {
    }
}
