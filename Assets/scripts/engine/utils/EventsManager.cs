namespace Engine
{
    using System;
    using System.Collections.Generic;

    public delegate void EventCallback(Actor dispatcher);

    public class EventObject
    {
        public EventCallback callback;
        public Object dispatcher;

        public EventObject(EventCallback callback, Actor dispatcher)
        {
            this.callback = callback;
            this.dispatcher = dispatcher;
        }


    }

    public class EventsManager
    {
        


        Dictionary<string, List<EventObject>> listeners = new Dictionary<string, List<EventObject>>();

        private static EventsManager instance;

        public EventsManager()
        {


        }

        public static EventsManager getInstance()
        {
            if (instance == null)
            {
                instance = new EventsManager();
            }

            return instance;
        }


        public void addListener(string eventId, EventObject obj)
        {
            if (!listeners.ContainsKey(eventId))
            {
                listeners[eventId] = new List<EventObject>();
            }

            listeners[eventId].Add(obj);
        }

        public void removeListener(string eventId, EventObject obj)
        {
            if (listeners.ContainsKey(eventId))
            {
                EventObject curr;
                List<EventObject> list = listeners[eventId];
                for (int i = 0; i < list.Count; i++)
                {
                    curr = list[i];
                    if (
                        curr.dispatcher == obj.dispatcher &&
                        curr.callback == obj.callback
                     )
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public void dispatchEvent(string eventId, Actor dispatcher)
        {
            if (listeners.ContainsKey(eventId))
            {
                List<EventObject> list = listeners[eventId];
                for (int i = 0; i < list.Count; i++)
                {
                    if (dispatcher == list[i].dispatcher)
                    {
                        list[i].callback(dispatcher);
                    }
                }
            }
        }
    }
}
