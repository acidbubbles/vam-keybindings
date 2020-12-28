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
            // TODO: Right now we're setting zero over and over again. Avoid it or make sure it's FAST
            var axisValue = map.chord.IsActive() ? Input.GetAxis(map.axisName) : 0;
            _remoteCommandsManager.UpdateValue(map.commandName, axisValue);
        }
    }
}
