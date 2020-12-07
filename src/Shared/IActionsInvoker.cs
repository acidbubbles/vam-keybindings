public interface IActionsInvoker
{
    void OnActionsProviderAvailable(JSONStorable storable);
    void OnActionsProviderDestroyed(JSONStorable storable);
}
