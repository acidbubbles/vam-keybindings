using System.Collections.Generic;

public interface IActionsProvider
{
    void OnBindingsListRequested(ICollection<object> bindings);
}
