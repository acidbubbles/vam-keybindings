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
            if (pluginsList == null)
            {
                SuperController.LogError($"Keybindings: Failed to find the plugins UI with panel {uiPanelCanvasName} and tab {tabName}");
                return false;
            }
            return ReloadPlugins(pluginsList, uidFilter);
        }

        private static bool ReloadPlugins(Transform pluginsList, string uidFilter = null)
        {
            var reloadButtons = new List<Button>();
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
                        reloadButtons.Add(pluginPanel.Find("ReloadButton").GetComponent<Button>());
                }
            }

            foreach (var reloadButton in reloadButtons)
            {
                reloadButton.onClick.Invoke();
            }

            return reloadButtons.Count > 0;
        }
    }
}
