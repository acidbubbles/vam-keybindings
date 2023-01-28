using UnityEngine;
using UnityEngine.Events;

public class PluginUISpy : MonoBehaviour
{
    public UnityEvent onSelected = new UnityEvent();

    public void OnEnable()
    {
        onSelected.Invoke();
    }

    public void OnDestroy()
    {
        onSelected.RemoveAllListeners();
    }
}
