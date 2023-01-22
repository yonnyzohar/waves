namespace Engine
{
    using System;
    using System.Collections.Generic;

    public class SequenceCommand : Command
    {
        private List<CmdConfigs.CmdInstance> commands;
        private int index = 0;

        public SequenceCommand(Context context, List<CmdConfigs.CmdInstance> _commands):base(context)
        {
            commands = _commands;
        }

        override public void execute(object _p = null)
        {
            runNextCommand();
        }

        private void runNextCommand()
        {
            if(index > commands.Count-1)
            {
                complete();
                return;
            }
            Context context = getContext();
            Command cmd = context.factory.getCommand(commands[index].id);
            cmd.addListener(Command.COMPLETE_MSG, onCommandComplete);
            cmd.execute(commands[index].instanceParams);
        }

        private void onCommandComplete(Actor disaptcher)
        {
            removeListener(Command.COMPLETE_MSG, onCommandComplete);
            index++;
            runNextCommand();
        }
    }
}

