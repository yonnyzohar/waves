using System;
using UnityEngine;
using System.Collections.Generic;

public class TimerAction
{
    public float durationSeconds;
    public float passedTime;
    public System.Action callback;

    public TimerAction(float _durationSeconds, System.Action _callback)
    {
        durationSeconds = _durationSeconds;
        passedTime = 0;
        callback = _callback;
    }
}



public class Timer
{
    public static List<TimerAction> timerActions = new List<TimerAction>();

    public static void CreateTimer(float durationSeconds, System.Action callback)
    {

        TimerAction action = new TimerAction(durationSeconds, callback);
        timerActions.Add(action);
    }

    public static void Update()
    {
        for(int i = 0; i < timerActions.Count; i++)
        {
            TimerAction t = timerActions[i];
            t.passedTime += Time.deltaTime;
            if(t.passedTime >= t.durationSeconds)
            {
                t.callback();
                timerActions.RemoveAt(i);
            }

        }
    }


}
