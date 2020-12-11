using System.Collections.Generic;

public interface ICommandsProvider
{
    void OnBindingsListRequested(ICollection<object> bindings);
}
