namespace Engine
{
    using UnityEngine;
    using System;
    public class Factory
    {
        CmdConfigs commands;
        Context context;

        public Factory(CmdConfigs _commands, Context _context)
        {
            context = _context;
            commands = _commands;
        }

        public Command getCommand(string id)
        {
            if(commands.definitons.ContainsKey(id))
            {
                Type CLS = commands.definitons[id].getCls();
                object p = commands.definitons[id].getParams();
                return (Command)Activator.CreateInstance(CLS, context, p);
            }
            
            Debug.Log(id + " does not exist in factory!");
            
            return null;
            
        }
    }

}
