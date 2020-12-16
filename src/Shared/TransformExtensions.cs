using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public static class TransformExtensions
    {
        public static bool ReloadPlugins(this Transform transform, string uiPanelCanvasName, string tabName, string uidFilter = null)
        {
            var pluginsList = transform
                .Find(uiPanelCanvasName)?
                .Find("Panel")?
                .Find("Content")?
                .Find(tabName)?
                .Find("Scroll View")?
                .Find("Viewport")?
                .Find("Content");
            if (pluginsList != null)
                return ReloadPlugins(pluginsList, uidFilter);
            SuperController.LogError($"Keybindings: Failed to find the plugins UI with panel {uiPanelCanvasName} and tab {tabName}");
            return false;
        }

        private static bool ReloadPlugins(Transform pluginsList, string uidFilter = null)
        {
            var reloadButtons = new List<KeyValuePair<string, Button>>();
            for (var i = 0; i < pluginsList.childCount; i++)
            {
                var pluginPanel = pluginsList.GetChild(i);
                var pluginPanelContent = pluginPanel.Find("Content");
                for (var j = 0; j < pluginPanelContent.childCount; j++)
                {
                    var scriptPanel = pluginPanelContent.GetChild(j);
                    var uid = scriptPanel
                        .Find("UID")
                        .GetComponent<Text>()
                        .text;
                    if (uidFilter == null || uidFilter == uid)
                        reloadButtons.Add(new KeyValuePair<string, Button>(uid, pluginPanel.Find("ReloadButton").GetComponent<Button>()));
                }
            }

            foreach (var reloadButton in reloadButtons)
            {
                try
                {
                    reloadButton.Value.onClick.Invoke();
                }
                catch(Exception exc)
                {
                    SuperController.LogError($"Keybindings: Failed reloading plugin {reloadButton.Key}: {exc}");
                }
            }

            return reloadButtons.Count > 0;
        }
    }
}
