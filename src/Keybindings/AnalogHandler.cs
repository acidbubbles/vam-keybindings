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

    public void Update()
    {
        for (var i = 0; i < _analogMapManager.maps.Count; i++)
        {
            var map = _analogMapManager.maps[i];
            var axisValue = map.chord.IsActive() ? Input.GetAxis(map.axisName) : 0;
            if (axisValue != 0)
            {
                map.isActive = true;
                _remoteCommandsManager.UpdateValue(map.commandName, axisValue);
            }
            else if (axisValue == 0)
            {
                if (map.isActive)
                {
                    _remoteCommandsManager.UpdateValue(map.commandName, 0);
                    map.isActive = false;
                }
            }
        }
    }
}
