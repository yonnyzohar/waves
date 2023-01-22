namespace RTS
{
    using UnityEngine;
    using System;
    public class MyCmd : Engine.Command
    {
        public MyCmd(RTSContext _context, object definitionParams = null) :base(_context)
        {
            
        }
        override public void execute(object _p = null)
        {
            Debug.Log("This is a command! " + (string)_p);
            complete();
        }
    }

}
