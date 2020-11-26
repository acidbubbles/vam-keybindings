using System.Collections.Generic;

public interface IActionsProvider
{
    void OnBindingsListRequested(List<object> bindings);
}
