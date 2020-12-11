public interface ICommandInvoker
{
    JSONStorable storable { get; }
    string name { get; }
    string label { get; }
    void Invoke();
}
