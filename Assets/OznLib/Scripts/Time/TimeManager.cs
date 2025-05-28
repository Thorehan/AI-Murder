using System.Collections;
using UnityEngine;

/// <summary>
/// it does it's job, dw
/// </summary>
public static class TimeManager
{
    static float TimeScale = 1;
    public static bool Pause = false;

    public static float TimeCalculation(float entry)
    {
        return Pause ? 0 : entry * TimeScale;
    }
    public static void ChangeScale(float modifier)
    {
        TimeScale = modifier;
    }
    public static Vector3 TimeCalculation(Vector3 entry)
    {
        return Pause ? Vector3.zero : entry * TimeScale;
    }
}
