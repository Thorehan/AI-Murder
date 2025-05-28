using System;
using UnityEngine;

public class UCoolDown
{
    public float Cooldown;
    float lastTime = 0;

    float timef => Time.deltaTime;

    public UCoolDown(float cooldown)
    {
        Cooldown = cooldown;
    }
    public bool CheckCD(float time)
    {
        if (lastTime + Cooldown < time)
        {
            lastTime = time;
            return true;
        }
        return false;
    }
    public bool CheckCD()
    {
        var time = timef;
        if (lastTime + Cooldown < time)
        {
            lastTime = time;
            return true;
        }
        return false;
    }

    public bool CheckIfCD(Action ifTrue)
    {
        if (CheckCD())
        {
            ifTrue?.Invoke();
            return true;
        }
        return false;
    }
    public void ResetCD()
    {
        lastTime = 0;
    }
    public float putCD()
    {
        lastTime = timef;
        return Cooldown;
    }
    public void putCD(float time)
    {
        lastTime = (timef - Cooldown) + time;
    }

    public static explicit operator bool(UCoolDown obj)
    {
        return obj.CheckCD();
    }
}

