using System.Linq;
using UnityEngine;

public static class BroadcastingUtil
{
    public static void BroadcastActionsAvailable(JSONStorable @this)
    {
        foreach (var atom in SuperController.singleton.GetAtoms())
        {
            foreach (var storable in atom.GetStorableIDs().Select(id => atom.GetStorableByID(id))
                .Where(s => s is MVRScript))
            {
                storable.SendMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), @this,
                    SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}
