using System;

public class PrintAction : IAction
{
    private readonly Func<string> _getMessage;

    public PrintAction(Func<string> getMessage)
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
