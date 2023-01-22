namespace Engine
{
    using System;
    public class Actor
    {
        Context context;
        public Actor(Context _context)
        {
            context = _context;
        }

        public Context getContext()
        {
            return context;
        }

        public Model getModel()
        {
            return context.getModel();
        }

        public void dispatchEvent(string eventId)
        {
            EventsManager.getInstance().dispatchEvent(eventId, this);
        }

        public void addListener(string eventId, EventCallback callback)
        {
            EventsManager.getInstance().addListener(eventId, new EventObject(callback, this));
        }

        public void removeListener(string eventId, EventCallback callback)
        {
            EventsManager.getInstance().removeListener(eventId, new EventObject(callback, this));
        }
    }

}
