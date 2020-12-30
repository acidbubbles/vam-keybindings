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
        if (LookInputModule.singleton.inputFieldActive)
            return;

        for (var i = 0; i < _analogMapManager.maps.Count; i++)
        {
            var map = _analogMapManager.maps[i];
            float axisValue;
            if(map.isAxis)
                axisValue = map.chord.IsActive() ? Input.GetAxis(map.axisName) : 0;
            else if (map.leftChord.IsActive())
                axisValue = -0.5f;
            else if (map.rightChord.IsActive())
                axisValue = 0.5f;
            else
                axisValue = 0f;

            if (axisValue != 0)
            {
                map.isActive = true;
                if (map.reversed) axisValue = -axisValue;
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
