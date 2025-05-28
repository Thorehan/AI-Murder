using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// change Time with TimeObject and your integration is done
/// you can stop game without stopping update calls, slow down or speed up the time and etc
/// </summary>
public class TimeObject : MonoBehaviour
{
#nullable enable
    static TimeObject? safeguard;
#nullable disable
    /// <summary>
    /// you dont actually need this as its all static functions and etc.
    /// this exist to generate one if there is none, you can easily use VibeCheck() 
    /// </summary>
    public static TimeObject instance
    {
        get
        {
            if(safeguard == null)
            {
                GameObject temp = new GameObject();
                instance = temp.AddComponent<TimeObject>();
            }
            return safeguard;
        }
        set
        {
            safeguard = value;
        }
    }
    static List<TimeModifier> timeModifiers = new List<TimeModifier> { };
    static float timedelta;
    public static float deltaTime
    {
        get
        {
            return timedelta;
        }
    }
    static float fixedtime;
    public static float fixedDeltaTime
    {
        get
        {
            return fixedtime;
        }
    }
    static float passedTime;
    public static float time
    {
        get => passedTime;
    }
    private void Awake()
    {

        Physics2D.simulationMode = SimulationMode2D.Script;
        Physics.simulationMode = SimulationMode.Script;
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void SimulatePhsic(float step)
    {
        Physics.Simulate(step);
        Physics2D.Simulate(step);
    }
    void Update()
    {
        timedelta = TimeManager.TimeCalculation(UnityEngine.Time.deltaTime);
    }
    private void FixedUpdate()
    {
        if (!TimeManager.Pause)
        {
            TimeModifierCalculation();
        }
        fixedtime = TimeManager.TimeCalculation(UnityEngine.Time.fixedDeltaTime);
        passedTime += TimeManager.TimeCalculation(UnityEngine.Time.fixedDeltaTime);
        if(!TimeManager.Pause)
        {
            SimulatePhsic(fixedtime);
        }
        


    }

    void TimeModifierCalculation()
    {
        float timer = 1;
        int i = 0;
        while(i<timeModifiers.Count)
        {
            TimeModifier mod = timeModifiers[i];
            //if you want the remaining time to decrease with multiplier, enable next line instead
            mod.timeRemaining -= TimeManager.Pause ? 0 : UnityEngine.Time.deltaTime; 
            //mod.timeRemaining -= timer * UnityEngine.Time.deltaTime;
            timer *= mod.amount;
            if(mod.timeRemaining < 0)
            {
                timeModifiers.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }

        TimeManager.ChangeScale(timer);
    }

    public static void AddModifier(TimeModifier mod)
    {
        timeModifiers.Add(mod);
    }
    
    public struct TimeModifier
    {
        public float timeRemaining;
        public float amount;
        public int id;
    }
    /// <summary>
    /// its a timestop that is 0.2 second long
    /// </summary>
    public static TimeModifier flash
    {
        get
        {
            return new TimeModifier { amount = 0, timeRemaining = 0.2f, id = 0};
        }
    }
    /// <summary>
    /// easy way to add 0.2 second stops
    /// </summary>
    public static void AddFlash()
    {
        AddModifier(flash);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="multiplier"></param>
    /// <param name="timer"></param>
    public static void AddTimeModifier(float multiplier, float timer)
    {
        AddModifier(new TimeModifier { amount = multiplier, timeRemaining = timer, id = 0 });
    }
    public static void AddTimeModifier(TimeModifier modifier)
    {
        AddModifier(modifier);
    }
    /// <summary>
    /// use this in a awake call, it will try to return the instance which generates itself if null.
    /// you dont need to recieve the instance
    /// </summary>
    /// <returns></returns>
    public static TimeObject VibeCheck()
    {
        return instance;
    }
}
