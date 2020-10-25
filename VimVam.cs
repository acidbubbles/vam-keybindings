using System;

public class VimVam : MVRScript
{
    public override void Init()
    {
        try
        {
            SuperController.LogMessage($"{nameof(VimVam)} initialized");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(Init)}: {e}");
        }
    }

    public void OnEnable()
    {
        try
        {
            SuperController.LogMessage($"{nameof(VimVam)} enabled");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(OnEnable)}: {e}");
        }
    }

    public void OnDisable()
    {
        try
        {
            SuperController.LogMessage($"{nameof(VimVam)} disabled");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(OnDisable)}: {e}");
        }
    }

    public void OnDestroy()
    {
        try
        {
            SuperController.LogMessage($"{nameof(VimVam)} destroyed");
        }
        catch (Exception e)
        {
            SuperController.LogError($"{nameof(VimVam)}.{nameof(OnDestroy)}: {e}");
        }
    }
}
