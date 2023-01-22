
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RTS
{

    public class RTSConfigs : Engine.CmdConfigs
    {
        
        public void init()
        {

            definitons.Add("myCmd", new CmdDefinition(typeof(MyCmd)));
            //this is instead of actually creating a MySequece commnand that
            //inheritst SequenceCommand. we set the cmd instances on the definiton
            definitons.Add("mSequence",
                new CmdDefinition(typeof(Engine.SequenceCommand),

                    //the list is a definiton param of the sequenceCommand
                    new List<CmdInstance> {
                        //these are instances of the myCmd with an init param
                        new CmdInstance("myCmd", "bob"),
                        new CmdInstance("myCmd", "moshe"),
                        new CmdInstance("myCmd", "mordechai"),
                        new CmdInstance("myCmd", "george"),
                        new CmdInstance("myCmd", "yonny")
                })
            );

        }

    }

}
