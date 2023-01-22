namespace Engine
{
    using System;
    public class Command : Actor
    {
        public static string COMPLETE_MSG = "COMMAND_COMPLETE";
        public Command(Context context, object definitionParams = null) :base(context)
        {
            
        }

        virtual public void execute(object instanceParams = null)
        {

        }

        public void complete()
        {
            dispatchEvent(Command.COMPLETE_MSG);
        }
    }

}
