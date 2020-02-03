using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnglesUtils
{
    // Convert angle between 0 and 360 degrees
    // When you have an angle in degrees, that you want to convert in the range of 0-360:
    public static float To360Angle(float angle)
    {
        while (angle < 0.0f) angle += 360.0f;
        while (angle >= 360.0f) angle -= 360.0f;
        return angle;
    }

    // Convert angle between 0 and 360 degrees
    // When you have an angle in degrees, that you want to convert in the range of 0-360:
    public static Vector3 To360Angle(Vector3 angles)
    {
        angles.x = To360Angle(angles.x);
        angles.y = To360Angle(angles.y);
        angles.z = To360Angle(angles.z);
        return angles;
    }

    // Convert angle between -180 and 180 degrees
    // If you want the angle to be between -180 and 180:
    public static float To180Angle(float angle)
    {
        while (angle < -180.0f) angle += 360.0f;
        while (angle >= 180.0f) angle -= 360.0f;
        return angle;
    }

    // Convert angle between -180 and 180 degrees
    // If you want the angle to be between -180 and 180:
    public static Vector3 To180Angle(Vector3 angles)
    {
        angles.x = To180Angle(angles.x);
        angles.y = To180Angle(angles.y);
        angles.z = To180Angle(angles.z);
        return angles;
    }

    // Convert mathematical angle to compass angle
    // Compass angles are slightly different from mathematical angles, 
    // because they start at the top(north and go clockwise, whereas 
    // mathematical angles start at the x-axis (east) and go counter-clockwise.
    public static float MathAngleToCompassAngle(float angle)
    {
        angle = 90.0f - angle;
        return To360Angle(angle);
    }

    // Lerp function for compass angles
    // When you gradually want to steer towards a target heading, you need a Lerp function.
    // But to slide from 350 degrees to 10 degrees should work like 350, 351, 352, ....359, 0, 1, 2, 3....10. 
    // And not the other way around going 350, 349, 348.....200...1000, 12, 11, 10.
    public static float CompassAngleLerp(float from, float to, float portion)
    {
        float dif = To180Angle(to - from);
        dif *= Mathf.Clamp01(portion);
        return To360Angle(from + dif);
    }
}
