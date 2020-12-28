public interface ICommandInvoker
{
    JSONStorable storable { get; }
    string commandName { get; }
    string ns { get; }
    string localName { get; }
}

public interface IActionCommandInvoker : ICommandInvoker
{
    void Invoke();
}

public interface IAnalogCommandInvoker : ICommandInvoker
{
    void UpdateValue(float value);
}
