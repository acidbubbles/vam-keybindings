using System.Linq;
using UnityEngine;

public static class BroadcastingUtil
{
    public static void BroadcastActionsAvailable(JSONStorable @this)
    {
        Broadcast(@this, nameof(IActionsInvoker.OnActionsProviderAvailable));
    }

    public static void BroadcastActionsDestroyed(JSONStorable @this)
    {
        Broadcast(@this, nameof(IActionsInvoker.OnActionsProviderDestroyed));
    }

    private static void Broadcast(JSONStorable @this, string method)
    {
        foreach (var atom in SuperController.singleton.GetAtoms())
        {
            foreach (var storable in atom.GetStorableIDs()
                .Select(id => atom.GetStorableByID(id))
                .Where(s => s is MVRScript))
            {
                storable.SendMessage(method, @this, SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
