using System;

/// <summary>
/// HOW TO USE
/// 
/// step one, make the put a function to action
/// step two, isEnabled will make it run or not
/// 
/// WHY TO USE
/// it's sometimes convenient, makes code a lil more readable
/// </summary>
public class ToggleAction
{
    bool enable = true;
    public Action action;
    Action runnerAction;
    public bool isEnabled
    {
        get
        {
            return enable;
        }
        set
        {
            enable = value;
            //runnerAction = value ? action : Empty;
            if (value == true)
            {
                runnerAction = action;
            }
            else
            {
                runnerAction = Empty;
            }
        }
    }


    public ToggleAction(Action Action)
    {
        action = Action;
        runnerAction = Action;
    }

    public void Call()
    {
        runnerAction?.Invoke();
    }

    static void Empty()
    {

    }

}


