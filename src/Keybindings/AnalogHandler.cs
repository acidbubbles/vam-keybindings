using System;
using UnityEngine;

public class AnalogHandler
{
    private readonly AnalogMapManager _analogMapManager;

    public AnalogHandler(AnalogMapManager analogMapManager)
    {
        _analogMapManager = analogMapManager;
    }

    public void FixedUpdate()
    {
        for (var i = 0; i < _analogMapManager.maps.Count; i++)
        {
            var map = _analogMapManager.maps[i];
            if (!map.chord.IsActive()) continue;
            var axisValue = Input.GetAxis(map.axisName);
            // TODO: Set command
            // SuperController.singleton.ClearMessages();
            // SuperController.LogMessage(axisValue.ToString());
        }
    }
}
