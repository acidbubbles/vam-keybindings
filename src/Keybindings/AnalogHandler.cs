using UnityEngine;

public class AnalogHandler
{
    private readonly RemoteCommandsManager _remoteCommandsManager;
    private readonly AnalogMapManager _analogMapManager;
    private readonly IKeybindingsSettings _settings;

    public AnalogHandler(RemoteCommandsManager remoteCommandsManager, AnalogMapManager analogMapManager, IKeybindingsSettings settings)
    {
        _remoteCommandsManager = remoteCommandsManager;
        _analogMapManager = analogMapManager;
        _settings = settings;
    }

    public void Update()
    {
        if (LookInputModule.singleton.inputFieldActive)
            return;

        for (var i = 0; i < _analogMapManager.maps.Count; i++)
        {
            var map = _analogMapManager.maps[i];
            float axisValue;
            if (map.isAxis)
                axisValue = map.chord.IsActive() ? map.GetAxis() : 0;
            else if (map.leftChord.IsActive())
                axisValue = -0.5f;
            else if (map.rightChord.IsActive())
                axisValue = 0.5f;
            else
                axisValue = 0f;

            if (axisValue != 0)
            {
                if (map.isMouse)
                {
                    if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                        continue;
                    axisValue *= _settings.mouseSensitivityJSON.val;
                }
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

    public void Leave()
    {
        foreach (var map in _analogMapManager.maps)
        {
            _remoteCommandsManager.UpdateValue(map.commandName, 0);
        }
    }
}
