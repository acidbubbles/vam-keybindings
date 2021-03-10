using System;
using System.Collections;
using System.Collections.Generic;
using MVR.FileManagementSecure;
using SimpleJSON;
using UnityEngine;

// ReSharper disable once InconsistentNaming
public class KeybindingsExtensions_AddPlugin : MVRScript
{
    private JSONStorableUrl _pluginJSON;
    private bool _loaded;

    public override void Init()
    {
        base.Init();

        RegisterUrl(_pluginJSON = new JSONStorableUrl("Plugin", null, "cs|cslist|dll", "Custom/Scripts")
        {
            beginBrowseWithObjectCallback = jsu =>
            {
                jsu.shortCuts = FileManagerSecure.GetShortCutsForDirectory("Custom/Scripts", true, true, true, true);
            },
            setCallbackFunction = val =>
            {
                // Potential values:
                // - Custom/Scripts/Dev/vam-timeline/VamTimeline.AtomAnimation.cslist
                // - AcidBubbles.Cornwall.2:/Custom/Scripts/AcidBubbles/Cornwall/Cornwall.cs
                SuperController.LogMessage(val);
                OnPluginsListChanged();
            },
            fileBrowseButton = CreateButton("Select").button
        });

        SuperController.singleton.StartCoroutine(DeferredInit());
    }

    private IEnumerator DeferredInit()
    {
        yield return new WaitForEndOfFrame();
        if (this == null) yield break;
        if (!_loaded) containingAtom.RestoreFromLast(this);
    }

    #region Save/Load

    public override void RestoreFromJSON(JSONClass jc, bool restorePhysical = true, bool restoreAppearance = true, JSONArray presetAtoms = null, bool setMissingToDefault = true)
    {
        base.RestoreFromJSON(jc, restorePhysical, restoreAppearance, presetAtoms, setMissingToDefault);
        _loaded = true;
        OnPluginsListChanged();
    }


    #endregion

    #region Keybindings integration

    private void OnPluginsListChanged()
    {
        transform.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderAvailable), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnDestroy()
    {
        transform.parent.BroadcastMessage(nameof(IActionsInvoker.OnActionsProviderDestroyed), this, SendMessageOptions.DontRequireReceiver);
    }

    public void OnBindingsListRequested(ICollection<object> bindings)
    {
        if (_pluginJSON.val == null) return;
        bindings.Add(new Dictionary<string, string>
             {
                 { "Namespace", "AddPlugin" }
             });
        bindings.Add(new JSONStorableAction("Timeline", () =>
        {
            SuperController.LogMessage("Adding " + _pluginJSON.val);
        }));
    }

    #endregion
}
