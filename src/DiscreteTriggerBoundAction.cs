public class DiscreteTriggerBoundAction : TriggerBoundAction, IBoundAction
{
    private readonly TriggerActionDiscrete _triggerAction;

    public DiscreteTriggerBoundAction(IPrefabManager prefabManager)
        : base(prefabManager)
    {
        _triggerAction = trigger.CreateDiscreteActionStartInternal();
    }

    public void Invoke()
    {
        _triggerAction.Trigger();
    }

    protected override void Open()
    {
        _triggerAction.OpenDetailPanel();

    }
}
