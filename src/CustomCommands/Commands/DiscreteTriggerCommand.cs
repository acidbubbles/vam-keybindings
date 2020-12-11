using System.Collections;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class DiscreteTriggerCommand : TriggerCommandBase, ICustomCommand
{
    public const string Type = "discreteTrigger";
    public string type => Type;

    public override string name => _triggerActionDiscrete.name;
    public object bindable => !string.IsNullOrEmpty(_invokingJsonStorableAction.name) ? _invokingJsonStorableAction : null;

    public string displayName
    {
        get
        {
            if (_triggerActionDiscrete.receiverAtom == null || _triggerActionDiscrete.receiver == null || string.IsNullOrEmpty(_triggerActionDiscrete.receiverTargetName))
                return "<i>[not set]</i>";

            string value;
            switch (_triggerActionDiscrete.actionType)
            {
                case JSONStorable.Type.Action:
                    value = null;
                    break;
                case JSONStorable.Type.Bool:
                    value = $" = {(_triggerActionDiscrete.boolValue ? "true" : "false")}";
                    break;
                case JSONStorable.Type.Color:
                    value = $" = (color)";
                    break;
                case JSONStorable.Type.Float:
                    value = $" = {_triggerActionDiscrete.floatValue}";
                    break;
                case JSONStorable.Type.String:
                    value = $" = '{_triggerActionDiscrete.stringValue}'";
                    break;
                case JSONStorable.Type.StringChooser:
                    value = $" = '{_triggerActionDiscrete.stringChooserValue}'";
                    break;
                case JSONStorable.Type.AudioClipAction:
                    value = $" = {_triggerActionDiscrete.audioClip?.displayName ?? "not set"}";
                    break;
                case JSONStorable.Type.PresetFilePathAction:
                    value = $" = '{_triggerActionDiscrete.presetFilePath}'";
                    break;
                case JSONStorable.Type.SceneFilePathAction:
                    value = $" = '{_triggerActionDiscrete.sceneFilePath}'";
                    break;
                default:
                    value = " (unknown type)";
                    break;
            }

            var commandName = (string.IsNullOrEmpty(_triggerActionDiscrete.name) ? "unnamed" : _triggerActionDiscrete.name);
            var targetLabel = $"{_triggerActionDiscrete.receiverAtom.name} / {_triggerActionDiscrete.receiver.name}";
            return $"<b>[{commandName}]</b> {targetLabel} / {_triggerActionDiscrete.receiverTargetName}{value}";

        }
    }

    private UnityEvent _onClose;
    private Coroutine _waitForCloseCo;
    private readonly TriggerActionDiscrete _triggerActionDiscrete;
    private readonly JSONStorableAction _invokingJsonStorableAction;

    public DiscreteTriggerCommand(Atom defaultAtom, IPrefabManager prefabManager)
        : base(prefabManager)
    {
        _triggerActionDiscrete = trigger.CreateDiscreteActionStartInternal();
        if (_triggerActionDiscrete.receiverAtom == null) _triggerActionDiscrete.receiverAtom = defaultAtom;
        if (_triggerActionDiscrete.receiver == null)
        {
            var defaultStorableId = defaultAtom.GetStorableIDs().FirstOrDefault(s => s.EndsWith("BindableActions"));
            if (defaultStorableId != null)
                _triggerActionDiscrete.receiver = defaultAtom.GetStorableByID(defaultStorableId);
        }
        _invokingJsonStorableAction = new JSONStorableAction("", Invoke);
    }

    public void Invoke()
    {
        _triggerActionDiscrete.Trigger();
    }

    protected override UnityEvent Open()
    {
        if (_waitForCloseCo != null)
            SuperController.singleton.StopCoroutine(_waitForCloseCo);
        _onClose?.Invoke();
        _triggerActionDiscrete.OpenDetailPanel();
        _onClose = new UnityEvent();
        _waitForCloseCo = SuperController.singleton.StartCoroutine(WaitForCloseCoroutine());
        return _onClose;
    }

    private IEnumerator WaitForCloseCoroutine()
    {
        yield return 0;
        while (_triggerActionDiscrete.detailPanelOpen)
            yield return 0;
        _invokingJsonStorableAction.name = _triggerActionDiscrete.name;
        _onClose.Invoke();
        _onClose.RemoveAllListeners();
        _onClose = null;
    }

    public override JSONClass GetJSON()
    {
        return _triggerActionDiscrete.GetJSON();
    }

    public override void RestoreFromJSON(JSONClass json)
    {
        _triggerActionDiscrete.RestoreFromJSON(json);
        _invokingJsonStorableAction.name = _triggerActionDiscrete.name;
    }
}
