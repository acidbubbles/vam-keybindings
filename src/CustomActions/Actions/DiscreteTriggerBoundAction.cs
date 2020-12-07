using System.Collections;
using System.Linq;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

public class DiscreteTriggerBoundAction : TriggerBoundAction, IBoundAction
{
    public const string Type = "discreteTrigger";
    public string type => Type;

    public override string name => _triggerAction.name;
    public object bindable => !string.IsNullOrEmpty(_jsa.name) ? _jsa : null;

    public string displayName
    {
        get
        {
            if (_triggerAction.receiverAtom == null || _triggerAction.receiver == null || string.IsNullOrEmpty(_triggerAction.receiverTargetName))
                return "<i>[not set]</i>";

            string value;
            switch (_triggerAction.actionType)
            {
                case JSONStorable.Type.Action:
                    value = null;
                    break;
                case JSONStorable.Type.Bool:
                    value = $" = {(_triggerAction.boolValue ? "true" : "false")}";
                    break;
                case JSONStorable.Type.Color:
                    value = $" = (color)";
                    break;
                case JSONStorable.Type.Float:
                    value = $" = {_triggerAction.floatValue}";
                    break;
                case JSONStorable.Type.String:
                    value = $" = '{_triggerAction.stringValue}'";
                    break;
                case JSONStorable.Type.StringChooser:
                    value = $" = '{_triggerAction.stringChooserValue}'";
                    break;
                case JSONStorable.Type.AudioClipAction:
                    value = $" = {_triggerAction.audioClip?.displayName ?? "not set"}";
                    break;
                case JSONStorable.Type.PresetFilePathAction:
                    value = $" = '{_triggerAction.presetFilePath}'";
                    break;
                case JSONStorable.Type.SceneFilePathAction:
                    value = $" = '{_triggerAction.sceneFilePath}'";
                    break;
                default:
                    value = " (unknown type)";
                    break;
            }

            var actionName = (string.IsNullOrEmpty(_triggerAction.name) ? "unnamed" : _triggerAction.name);
            var targetLabel = $"{_triggerAction.receiverAtom.name} / {_triggerAction.receiver.name}";
            return $"<b>[{actionName}]</b> {targetLabel} / {_triggerAction.receiverTargetName}{value}";

        }
    }

    private UnityEvent _onClose;
    private Coroutine _waitForCloseCo;
    private readonly TriggerActionDiscrete _triggerAction;
    private readonly JSONStorableAction _jsa;

    public DiscreteTriggerBoundAction(Atom defaultAtom, IPrefabManager prefabManager)
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
        _jsa = new JSONStorableAction("", Invoke);
    }

    public void Invoke()
    {
        _triggerAction.Trigger();
    }

    protected override UnityEvent Open()
    {
        if (_waitForCloseCo != null)
            SuperController.singleton.StopCoroutine(_waitForCloseCo);
        _onClose?.Invoke();
        _triggerAction.OpenDetailPanel();
        _onClose = new UnityEvent();
        _waitForCloseCo = SuperController.singleton.StartCoroutine(WaitForCloseCoroutine());
        return _onClose;
    }

    private IEnumerator WaitForCloseCoroutine()
    {
        yield return 0;
        while (_triggerAction.detailPanelOpen)
            yield return 0;
        _jsa.name = _triggerAction.name;
        _onClose.Invoke();
        _onClose.RemoveAllListeners();
        _onClose = null;
    }

    public override JSONClass GetJSON()
    {
        return _triggerAction.GetJSON();
    }

    public override void RestoreFromJSON(JSONClass json)
    {
        _triggerAction.RestoreFromJSON(json);
        _jsa.name = _triggerAction.name;
    }
}
