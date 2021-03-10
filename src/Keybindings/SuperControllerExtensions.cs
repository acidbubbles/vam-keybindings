using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class SuperControllerExtensions
{
    // NOTE: Most of this comes from Virt-A-Mate's implementation.

    public static string CreateUID(this SuperController sc, string source)
    {
        var uids = new HashSet<string>(sc.GetAtomUIDs());
        var hashIndex = source.LastIndexOf('#');
        var startAt = 0;
        if (hashIndex == -1)
        {
            if (!uids.Contains(source)) return source;
            source += "#";
            startAt = 2;
        }
        else
        {
            if (int.TryParse(source.Substring(hashIndex + 1), out startAt))
                startAt++;
            else
                startAt = 2;
            source = source.Substring(0, hashIndex + 1);
        }

        for (var i = startAt; i < 1000; i++)
        {
            var uid = source + i;
            if (!uids.Contains(uid)) return uid;
        }

        return source + Guid.NewGuid();
    }

    public static void CameraPan(this SuperController sc, float val, Vector3 direction)
    {
        var navigationRig = sc.navigationRig;
        var position = sc.navigationRig.position;
        position += direction * ((0f - val) * 0.03f);
        var up = navigationRig.up;
        var delta = position - navigationRig.position;
        var upDelta = Vector3.Dot(delta, up);
        position += up * (0f - upDelta);
        navigationRig.position = position;
        sc.playerHeightAdjust += upDelta;
        sc.SyncMonitorRigPosition();
    }

    public static void CameraOrbitX(this SuperController sc, float val)
    {
        var monitorCenterCameraTransform = sc.MonitorCenterCamera.transform;
        var point = monitorCenterCameraTransform.position + monitorCenterCameraTransform.forward * sc.focusDistance;
        sc.navigationRig.RotateAround(point, sc.navigationRig.up, val * 2f);
        sc.SyncMonitorRigPosition();
    }

    public static void CameraOrbitY(this SuperController sc, float val)
    {
        var monitorCenterCameraTransform = sc.MonitorCenterCamera.transform;
        var navigationRig = sc.navigationRig;
        var focusDistance = sc.focusDistance;
        var position = monitorCenterCameraTransform.position;
        var vector = position + monitorCenterCameraTransform.forward * focusDistance;
        var up = navigationRig.up;
        var a = position - up * (val * 0.1f * focusDistance);
        var a2 = a - vector;
        a2.Normalize();
        a = vector + a2 * focusDistance;
        var vector2 = a - position;
        var position2 = navigationRig.position + vector2;
        var num = Vector3.Dot(vector2, up);
        position2 += up * (0f - num);
        navigationRig.position = position2;
        sc.playerHeightAdjust += num;
        monitorCenterCameraTransform.LookAt(vector);
        var localEulerAngles = monitorCenterCameraTransform.localEulerAngles;
        localEulerAngles.y = 0f;
        localEulerAngles.z = 0f;
        monitorCenterCameraTransform.localEulerAngles = localEulerAngles;
        sc.SyncMonitorRigPosition();
    }

    public static void CameraDollyZoom(this SuperController sc, float val)
    {
        var num3 = 0.1f;
        if (val < -0.5f)
        {
            num3 = 0f - num3;
        }
        var forward = sc.MonitorCenterCamera.transform.forward;
        var vector3 = forward * (num3 * sc.focusDistance);
        var position4 = sc.navigationRig.position + vector3;
        sc.focusDistance *= 1f - num3;
        var up3 = sc.navigationRig.up;
        var num4 = Vector3.Dot(vector3, up3);
        position4 += up3 * (0f - num4);
        sc.navigationRig.position = position4;
        sc.playerHeightAdjust += num4;
        sc.SyncMonitorRigPosition();
    }
}
