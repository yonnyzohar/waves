using System;
using System.Collections.Generic;

public class EventObject
{
    public System.Action callback;
    public Object dispatcher;

    public EventObject(System.Action callback, Object dispatcher = null)
    {
        this.callback = callback;
        this.dispatcher = dispatcher;
    }


}

public class EventsManager
{
    Dictionary<string,List<EventObject>> listeners = new Dictionary<string, List<EventObject>>();

    private static EventsManager instance;

    public EventsManager()
	{
        

    }

    public static EventsManager getInstance()
    {
        if(instance == null)
        {
            instance = new EventsManager();
        }

        return instance;
    }


    public void addListener(string eventId, EventObject obj)
    {
        if(!listeners.ContainsKey(eventId))
        {
            listeners[eventId] = new List<EventObject>();
        }

        listeners[eventId].Add(obj);
    }

    public void removeListener(string eventId, EventObject obj)
    {
        if(listeners.ContainsKey(eventId))
        {
            List<EventObject> list = listeners[eventId];
            for (int i = 0; i < list.Count; i++)
            {
                if(list[i] == obj)
                {
                    list.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public void dispatchEvent(string eventId, Object dispatcher = null)
    {
        if (listeners.ContainsKey(eventId))
        {
            List<EventObject> list = listeners[eventId];
            for (int i = 0; i < list.Count; i++)
            {
                if(dispatcher == null)
                {
                    if(list[i].dispatcher == null)
                    {
                        list[i].callback();
                    }
                    
                }
                else
                {
                    if(dispatcher == list[i].dispatcher)
                    {
                        list[i].callback();
                    }
                }
                
            }
        }
    }

}