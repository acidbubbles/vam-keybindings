using System.Linq;
using SimpleJSON;

public class DiscreteTriggerBoundAction : TriggerBoundAction, IBoundAction
{
    public const string Type = "discreteTrigger";
    public string type => Type;

    private readonly TriggerActionDiscrete _triggerAction;

    public DiscreteTriggerBoundAction(IPrefabManager prefabManager, Atom defaultAtom)
        : base(prefabManager)
    {
        _triggerAction = trigger.CreateDiscreteActionStartInternal();
        if (_triggerAction.receiverAtom == null) _triggerAction.receiverAtom = defaultAtom;
        if (_triggerAction.receiver == null)
        {
            var defaultStorableId = defaultAtom.GetStorableIDs().FirstOrDefault(s => s.EndsWith("BindableActions"));
            if (defaultStorableId != null)
                _triggerAction.receiver = defaultAtom.GetStorableByID(defaultStorableId);
        }
    }

    public void Invoke()
    {
        _triggerAction.Trigger();
    }

    protected override void Open()
    {
        _triggerAction.OpenDetailPanel();
    }

    public override JSONClass GetJSON()
    {
        return _triggerAction.GetJSON();
    }

    public override void RestoreFromJSON(JSONClass json)
    {
        _triggerAction.RestoreFromJSON(json);
    }
}
