namespace Engine
{
    using System;
    using UnityEngine;
    using System.Collections.Generic;

    public delegate void UpdateTick(float per);

    public class TimerAction
    {
        public float durationSeconds;
        public float passedTime;
        public System.Action callback;
        public UpdateTick update;


        public TimerAction(float _durationSeconds, System.Action _callback, UpdateTick _update = null)
        {
            durationSeconds = _durationSeconds;
            passedTime = 0;
            callback = _callback;
            update = _update;
        }
    }



    public class Timer
    {
        public static List<TimerAction> timerActions = new List<TimerAction>();

        public static void CreateTimer(float durationSeconds, System.Action callback, UpdateTick _update = null)
        {
            TimerAction action = new TimerAction(durationSeconds, callback, _update);
            timerActions.Add(action);
        }

        public static void Update()
        {
            for (int i = 0; i < timerActions.Count; i++)
            {
                TimerAction t = timerActions[i];
                t.passedTime += Time.deltaTime;
                if(t.update != null)
                {
                    t.update(t.passedTime / t.durationSeconds);
                }
                
                if (t.passedTime >= t.durationSeconds)
                {
                    t.callback();
                    timerActions.RemoveAt(i);
                }

            }
        }


    }

}
