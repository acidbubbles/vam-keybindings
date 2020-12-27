using System;
using System.Linq;
using UnityEngine;

public class AnalogHandler
{
    private readonly RemoteCommandsManager _remoteCommandsManager;
    private readonly AnalogMapManager _analogMapManager;

    public AnalogHandler(RemoteCommandsManager remoteCommandsManager, AnalogMapManager analogMapManager)
    {
        _remoteCommandsManager = remoteCommandsManager;
        _analogMapManager = analogMapManager;
    }

    public void FixedUpdate()
    {
        for (var i = 0; i < _analogMapManager.maps.Count; i++)
        {
            var map = _analogMapManager.maps[i];
            // TODO: Right now we're setting zero over and over again. Avoid it or make sure it's FAST
            float normalized;
            if (map.chord.IsActive())
            {
                var axisValue = Input.GetAxis(map.axisName);
                normalized = axisValue * Time.fixedUnscaledDeltaTime;
            }
            else
            {
                normalized = 0;
            }
            _remoteCommandsManager.UpdateValue(map.commandName, normalized);
        }
    }
}
