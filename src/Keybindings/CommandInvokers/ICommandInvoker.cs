public interface ICommandInvoker
{
    JSONStorable storable { get; }
    string commandName { get; }
    string ns { get; }
    string localName { get; }
    void Invoke();
}
